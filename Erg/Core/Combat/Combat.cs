using Erg.Core.World;
using static Erg.Core.Messages.Perspective;

namespace Erg.Core.Combat;

public static class Combat
{
    public static void UnarmedAttack(Critter attacker, Critter defender, Session session)
    {
        var random = session.Random;
        var messages = session.Messages;

        int damage = attacker.UnarmedDamage.Roll(random);
        defender.TakeDamage(damage, attacker);

        bool seeAttacker = session.CanPlayerSee(attacker);
        bool seeDefender = session.CanPlayerSee(defender);

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
            // Grant XP to attacker (level up messages handled inside)
            attacker.GainExperience(defender.Value, session);

            // Death message (only if defender visible)
            if (seeDefender)
            {
                messages.Add($"{NameOf(defender)} {Verb(defender, "die", "dies")}!");
            }
        }
    }
}
