using Erg.Core.Game;

namespace Erg.Core;

public static class Combat
{
    public static void MeleeAttack(Critter attacker, Critter defender, MessageBuffer messages)
    {
        int damage = attacker.MeleeDamage;
        defender.TakeDamage(damage, attacker);

        messages.Add($"{attacker.Name} hits {defender.Name} for {damage} damage.");

        if (!defender.IsAlive)
        {
            messages.Add($"{defender.Name} dies!");
        }
    }
}
