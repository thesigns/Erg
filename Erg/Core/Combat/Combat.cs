using Erg.Core.Game;

namespace Erg.Core.Combat;

public static class Combat
{
    public static void MeleeAttack(Critter attacker, Critter defender, MessageBuffer messages, Random random)
    {
        int damage = attacker.MeleeDamage.Roll(random);
        defender.TakeDamage(damage, attacker);

        messages.Add($"{attacker.Name} hits {defender.Name} for {damage} damage.");

        if (!defender.IsAlive)
        {
            messages.Add($"{defender.Name} dies!");
        }
    }
}
