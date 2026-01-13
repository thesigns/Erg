using System;
using System.Collections.Generic;
using System.Linq;
using Erg.Core.Types;
using Erg.Core.World;
using Erg.Core.World.Behaviors;

public abstract class Critter : Entity
{
    public int Speed { get; protected set; }   // np. 100 = normal
    public int Energy { get; protected set; }  // akumulowana
    public Inventory Inventory { get; } = new();
    public IBehavior? Behavior { get; protected set; }

    // Hit Points
    public int MaxHitPoints { get; protected set; }
    public int HitPoints { get; protected set; }
    public bool IsAlive => HitPoints > 0;

    // Combat
    public Dice MeleeDamage { get; protected set; }
    public Critter? KilledBy { get; private set; }

    // Stos wrogow - wrog na gorze to aktualny cel
    private readonly List<Critter> _enemies = new();
    public IReadOnlyList<Critter> Enemies => _enemies;
    public Critter? CurrentEnemy => _enemies.Count > 0 ? _enemies[^1] : null;

    protected Critter(
        string name,
        int x,
        int y,
        char character,
        uint fg,
        uint bg,
        int speed,
        int maxHitPoints = 10,
        Dice? meleeDamage = null,
        IBehavior? behavior = null)
        : base(name, x, y, character, fg, bg)
    {
        Speed = speed;
        Energy = 0;
        MaxHitPoints = maxHitPoints;
        HitPoints = maxHitPoints;
        MeleeDamage = meleeDamage ?? new Dice(1, 4);
        Behavior = behavior;
    }

    public void GainEnergy()
    {
        Energy += Speed;
    }

    public bool CanAct()
    {
        return Energy >= 0;
    }

    public void SpendEnergy(int cost)
    {
        Energy -= cost;
    }

    // Dodaj wroga na stos (jesli juz jest - przeniesc na gore)
    public void AddEnemy(Critter enemy)
    {
        _enemies.Remove(enemy);
        _enemies.Add(enemy);
    }

    // Usun wroga ze stosu (np. gdy umrze)
    public void RemoveEnemy(Critter enemy)
    {
        _enemies.Remove(enemy);
    }

    // Otrzymaj obrazenia
    public void TakeDamage(int damage, Critter? attacker = null)
    {
        HitPoints = Math.Max(0, HitPoints - damage);
        if (attacker != null)
        {
            AddEnemy(attacker);
            if (!IsAlive)
                KilledBy = attacker;
        }
    }

    // Ulecz
    public void Heal(int amount)
    {
        HitPoints = Math.Min(MaxHitPoints, HitPoints + amount);
    }

    // Wywoływane przy śmierci - upuszcza inventory
    public virtual void OnDeath(Area area)
    {
        foreach (var item in Inventory.Items.ToList())
        {
            item.MoveTo(X, Y);
            area.AddItem(item);
        }
        Inventory.Clear();
    }
}