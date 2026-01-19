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

        // Surprise attack logic:
        // - NPC: surprise if attacker not in defender's enemies
        // - Player: surprise only if can't see attacker AND attacker not in enemies
        bool isSurprise;
        if (defender is Player)
            isSurprise = !seeAttacker && !defender.Enemies.Contains(attacker);
        else
            isSurprise = !defender.Enemies.Contains(attacker);

        // Hit chance: Attack / (Attack + Defense)
        // Surprise: min 50%, normal: min 5%
        double hitChance = (double)attacker.UnarmedAttack / (attacker.UnarmedAttack + defender.UnarmedDefense);
        double minHitChance = isSurprise ? 0.50 : 0.05;
        hitChance = Math.Clamp(hitChance, minHitChance, 0.95);

        bool hit = random.NextDouble() < hitChance;

        // Combat training (both combatants learn from fighting)
        const double CombatTrainingAmount = 0.01;
        attacker.TrainUnarmed(CombatTrainingAmount, session);
        attacker.TrainAttack(CombatTrainingAmount, session);
        defender.TrainUnarmed(CombatTrainingAmount, session);
        defender.TrainDefense(CombatTrainingAmount, session);

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
        // Surprise: max damage, normal: roll
        int damage = isSurprise
            ? attacker.UnarmedDamage.Max() + attacker.DamageBonus
            : attacker.UnarmedDamage.Roll(random) + attacker.DamageBonus;
        defender.TakeDamage(damage, attacker);

        // Damage info only when player is directly involved
        bool playerInvolved = attacker is Player || defender is Player;
        string surpriseText = isSurprise ? " by surprise" : "";

        // Attack message based on visibility
        if (seeAttacker && seeDefender)
        {
            if (playerInvolved)
                messages.Add($"{NameOf(attacker)} {Verb(attacker, "hit", "hits")} {NameOf(defender, false)}{surpriseText} for {damage} damage!");
            else
                messages.Add($"{NameOf(attacker)} {Verb(attacker, "hit", "hits")} {NameOf(defender, false)}{surpriseText}.");
        }
        else if (seeAttacker && !seeDefender)
        {
            // Attacker visible only - no damage info
            messages.Add($"{NameOf(attacker)} {Verb(attacker, "hit", "hits")} something{surpriseText}.");
        }
        else if (!seeAttacker && seeDefender)
        {
            // Defender visible only
            if (defender is Player)
                messages.Add($"Something hits {NameOf(defender, false)}{surpriseText} for {damage} damage!");
            else
                messages.Add($"Something hits {NameOf(defender, false)}{surpriseText}.");
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
