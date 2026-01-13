using Erg.Core.World;

namespace Erg.Core.Combat;

public static class Combat
{
    public static void MeleeAttack(Critter attacker, Critter defender, Session session)
    {
        var random = session.Random;
        var messages = session.Messages;

        int damage = attacker.MeleeDamage.Roll(random);
        defender.TakeDamage(damage, attacker);

        bool seeAttacker = session.CanPlayerSee(attacker);
        bool seeDefender = session.CanPlayerSee(defender);

        // Damage info only when player is directly involved
        bool playerInvolved = attacker is Player || defender is Player;

        // Attack message based on visibility
        if (seeAttacker && seeDefender)
        {
            if (playerInvolved)
                messages.Add($"{attacker.Name} hits {defender.Name} for {damage} damage.");
            else
                messages.Add($"{attacker.Name} hits {defender.Name}.");
        }
        else if (seeAttacker && !seeDefender)
        {
            // Attacker visible only - no damage info
            messages.Add($"{attacker.Name} hits something.");
        }
        else if (!seeAttacker && seeDefender)
        {
            // Defender visible only
            if (defender is Player)
                messages.Add($"Something hits {defender.Name} for {damage} damage.");
            else
                messages.Add($"Something hits {defender.Name}.");
        }
        // Neither visible: no message (silent combat)

        // Death message (only if defender visible)
        if (!defender.IsAlive && seeDefender)
        {
            messages.Add($"{defender.Name} dies!");
        }
    }
}
