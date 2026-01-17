using System;
using System.Collections.Generic;
using System.Linq;
using Erg.Core.Types;
using Erg.Core.World;
using Erg.Core.World.Behaviors;
using Erg.Core.World.Pathfinding;
using Erg.Core.Systems;

public abstract class Critter : Entity
{
    public int Speed { get; protected set; }   // np. 100 = normal
    public int Energy { get; protected set; }  // akumulowana
    public Inventory Inventory { get; } = new();
    public Attributes Attributes { get; }
    public Species Species { get; protected set; } = Species.Human;
    public IBehavior? Behavior { get; protected set; }

    // Hit Points
    public int MaxHitPoints { get; protected set; }
    public int HitPoints { get; protected set; }
    public bool IsAlive => HitPoints > 0;

    // Combat
    public Dice UnarmedDamage { get; protected set; }
    public Critter? KilledBy { get; private set; }

    // Pronouns
    public PronounSet Pronouns { get; protected set; } = PronounSet.It;

    // Movement
    public Locomotion Locomotion { get; protected set; } = Locomotion.Terrestrial;

    // Abilities
    public virtual bool CanOpenDoor => false;

    // Vision
    public virtual int SightRange => 8;

    /// <summary>
    /// Checks if this critter can see the given tile (within range and has line of sight).
    /// </summary>
    public bool CanSeeTile(Area area, int x, int y)
    {
        int dx = Math.Abs(x - X);
        int dy = Math.Abs(y - Y);
        if (Math.Max(dx, dy) > SightRange) return false;
        return LineOfSight.CanSee(area, X, Y, x, y);
    }

    /// <summary>
    /// Checks if this critter can see another critter.
    /// Future: will also check for blindness, invisibility, stealth, etc.
    /// </summary>
    public bool CanSeeCritter(Area area, Critter target)
    {
        // Future considerations:
        // - if (this.IsBlind) return false;
        // - if (target.IsInvisible) return false;
        // - if (target.IsSneaking && !this.CanDetectStealth) return false;
        return CanSeeTile(area, target.X, target.Y);
    }

    // Experience
    public int ExperienceLevel { get; protected set; } = 1;
    public int ExperiencePoints { get; protected set; } = 0;
    public int ExperienceToNextLevel => ExperienceConfig.CalculateXPForLevel(ExperienceLevel);
    public int ExperienceMultiplier { get; protected set; } = 100; // 100 = x1.0

    // Value
    public int BaseValue { get; protected set; } = 10;
    public int Value => BaseValue * ExperienceLevel;

    // Passive regeneration
    public float RegenChancePerSegment { get; protected set; } = 0.001f;
    public Dice RegenDice { get; protected set; } = new Dice(1, 1);
    public int PendingRegen { get; protected set; } = 0;

    // ========== Attribute Training ==========

    /// <summary>
    /// Trains an attribute using the species-specific training function.
    /// </summary>
    public void TrainAttribute(Erg.Core.Systems.Attribute attr, double amount, Session session)
    {
        var function = Species.GetTrainingFunction(attr, Attributes);
        attr.Train(amount, function, session.TrainingSpeed);
    }

    public void TrainStrength(double amount, Session session)
        => Attributes.Strength.Train(amount, Species.StrengthTraining, session.TrainingSpeed);

    public void TrainEndurance(double amount, Session session)
        => Attributes.Endurance.Train(amount, Species.EnduranceTraining, session.TrainingSpeed);

    public void TrainAgility(double amount, Session session)
        => Attributes.Agility.Train(amount, Species.AgilityTraining, session.TrainingSpeed);

    public void TrainPerception(double amount, Session session)
        => Attributes.Perception.Train(amount, Species.PerceptionTraining, session.TrainingSpeed);

    public void TrainIntelligence(double amount, Session session)
        => Attributes.Intelligence.Train(amount, Species.IntelligenceTraining, session.TrainingSpeed);

    public void TrainWillpower(double amount, Session session)
        => Attributes.Willpower.Train(amount, Species.WillpowerTraining, session.TrainingSpeed);

    public void TrainCharisma(double amount, Session session)
        => Attributes.Charisma.Train(amount, Species.CharismaTraining, session.TrainingSpeed);

    /// <summary>
    /// Checks if this critter can enter the given tile based on Locomotion.
    /// </summary>
    public virtual bool CanEnterTile(Tile tile)
    {
        return Locomotion switch
        {
            Locomotion.Terrestrial => tile.Walkable && !tile.Swimmable,
            Locomotion.Amphibious => tile.Walkable || tile.Swimmable,
            Locomotion.Semiaquatic => tile.Walkable || tile.Swimmable,
            Locomotion.Aquatic => tile.Swimmable,
            Locomotion.Aerial => tile.Flyable,
            _ => tile.Walkable
        };
    }

    /// <summary>
    /// Returns movement energy cost multiplier based on terrain.
    /// 1.0 = normal, greater than 1.0 = slower
    /// </summary>
    public virtual float GetMovementCostMultiplier(Tile tile)
    {
        if (Locomotion == Locomotion.Amphibious)
        {
            if (tile.Type == TileType.DeepWater) return 2.0f;      // Speed 50
            if (tile.Type == TileType.ShallowWater) return 1.25f;  // Speed 80
        }
        if (Locomotion == Locomotion.Semiaquatic)
        {
            // Penalty on land, comfortable in water
            if (!tile.Swimmable) return 2.0f;
        }
        return 1.0f;
    }

    /// <summary>
    /// Returns locomotion message for entering a tile, or null if none.
    /// </summary>
    public virtual string? GetLocomotionMessage(Tile tile)
    {
        if (Locomotion == Locomotion.Amphibious)
        {
            if (tile.Type == TileType.DeepWater) return "You swim.";
            if (tile.Type == TileType.ShallowWater) return "You wade knee-deep in water.";
        }
        if (Locomotion == Locomotion.Semiaquatic)
        {
            if (!tile.Swimmable) return "You lumber across the dry ground.";
        }
        return null;
    }

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
        Dice? unarmedDamage = null,
        IBehavior? behavior = null)
        : base(name, x, y, character, fg, bg)
    {
        Speed = speed;
        Energy = 0;
        MaxHitPoints = maxHitPoints;
        HitPoints = maxHitPoints;
        UnarmedDamage = unarmedDamage ?? new Dice(1, 4);
        Behavior = behavior;
        Attributes = new Attributes();
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

    // Passive regeneration - called each segment
    public void TryAccumulateRegen(Random random)
    {
        if (HitPoints >= MaxHitPoints) return;
        if (RegenChancePerSegment <= 0) return;

        if (random.NextDouble() < RegenChancePerSegment)
        {
            PendingRegen += RegenDice.Roll(random);
        }
    }

    // Apply accumulated regen before action
    public void ApplyPendingRegen()
    {
        if (PendingRegen > 0)
        {
            Heal(PendingRegen);
            PendingRegen = 0;
        }
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

    // Experience
    public void GainExperience(int baseAmount, Session? session = null)
    {
        int gained = baseAmount * ExperienceMultiplier / 100;
        ExperiencePoints += gained;
        OnGainExperience(gained);

        // Auto level-up with overflow (handles multiple level-ups)
        while (ExperiencePoints >= ExperienceToNextLevel)
        {
            ExperiencePoints -= ExperienceToNextLevel;
            ExperienceLevel++;
            OnLevelUp(ExperienceLevel);

            // Level up message (if session provided)
            if (session != null)
            {
                if (this is Player)
                    session.Messages.Add("You gained a level!");
                else if (session.CanPlayerSee(this))
                    session.Messages.Add($"The {Name} suddenly seems more powerful.");
            }
        }
    }

    protected virtual void OnGainExperience(int amount)
    {
        // Override in subclasses for XP gain reactions
    }

    protected virtual void OnLevelUp(int newLevel)
    {
        // +4 HP on level up
        MaxHitPoints += 4;
        HitPoints += 4;
    }
}