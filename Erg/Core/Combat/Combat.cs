using Erg.Core.World;
using static Erg.Core.Messages.Perspective;

namespace Erg.Core.Combat;

public static class Combat
{
    public static void UnarmedAttack(Critter attacker, Critter defender, Session session)
    {
        var random = session.Random;
        var messages = session.Messages;

        bool seeAttacker = session.CanPlayerSee(attacker);
        bool seeDefender = session.CanPlayerSee(defender);

        // Hit chance: Attack / (Attack + Defense), clamped to 5%-95%
        double hitChance = (double)attacker.UnarmedAttack / (attacker.UnarmedAttack + defender.UnarmedDefense);
        hitChance = Math.Clamp(hitChance, 0.05, 0.95);

        bool hit = random.NextDouble() < hitChance;

        if (!hit)
        {
            // Miss - no damage
            if (seeAttacker || seeDefender)
            {
                messages.Add($"{NameOf(attacker)} {Verb(attacker, "miss", "misses")} {NameOf(defender, false)}.");
            }
            return;
        }

        // Hit - calculate and apply damage
        int damage = attacker.UnarmedDamage.Roll(random) + attacker.DamageBonus;
        defender.TakeDamage(damage, attacker);

        // Damage info only when player is directly involved
        bool playerInvolved = attacker is Player || defender is Player;

        // Attack message based on visibility
        if (seeAttacker && seeDefender)
        {
            if (playerInvolved)
                messages.Add($"{NameOf(attacker)} {Verb(attacker, "hit", "hits")} {NameOf(defender, false)} for {damage} damage.");
            else
                messages.Add($"{NameOf(attacker)} {Verb(attacker, "hit", "hits")} {NameOf(defender, false)}.");
        }
        else if (seeAttacker && !seeDefender)
        {
            // Attacker visible only - no damage info
            messages.Add($"{NameOf(attacker)} {Verb(attacker, "hit", "hits")} something.");
        }
        else if (!seeAttacker && seeDefender)
        {
            // Defender visible only
            if (defender is Player)
                messages.Add($"Something hits {NameOf(defender, false)} for {damage} damage.");
            else
                messages.Add($"Something hits {NameOf(defender, false)}.");
        }
        // Neither visible: no message (silent combat)

        // Death handling
        if (!defender.IsAlive)
        {
            // Death message (only if defender visible)
            if (seeDefender)
            {
                messages.Add($"{NameOf(defender)} {Verb(defender, "die", "dies")}!");
            }
        }
    }
}
