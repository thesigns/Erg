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
    /// <summary>
    /// Speed calculated from Genus.BaseSpeed and Agility.
    /// Formula: BaseSpeed * (0.5 + Agility)
    /// </summary>
    public int Speed => (int)(Genus.BaseSpeed * (0.5 + Attributes.Agility.CurrentValue));
    public int Energy { get; protected set; }  // akumulowana
    public Inventory Inventory { get; } = new();
    public Attributes Attributes { get; }
    public Genus Genus { get; protected set; } = Genus.Human;
    public IBehavior? Behavior { get; protected set; }

    // Hit Points
    /// <summary>
    /// Max HP calculated from Genus.BaseMaxHitPoints and Endurance.
    /// Formula: BaseMaxHitPoints * (0.5 + Endurance)
    /// </summary>
    public int MaxHitPoints => (int)(Genus.BaseMaxHitPoints * (0.5 + Attributes.Endurance.CurrentValue));
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

    // Depth-based spawning
    /// <summary>
    /// Minimum dungeon depth at which this critter type can spawn.
    /// </summary>
    public virtual int MinDepth => 1;

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

    // Value (used for determining creature toughness/worth)
    public int BaseValue { get; protected set; } = 10;

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
        var function = Genus.GetTrainingFunction(attr, Attributes);
        attr.Train(amount, function, session.TrainingSpeed);
    }

    public void TrainStrength(double amount, Session session)
        => Attributes.Strength.Train(amount, Genus.StrengthTraining, session.TrainingSpeed);

    public void TrainEndurance(double amount, Session session)
        => Attributes.Endurance.Train(amount, Genus.EnduranceTraining, session.TrainingSpeed);

    public void TrainAgility(double amount, Session session)
        => Attributes.Agility.Train(amount, Genus.AgilityTraining, session.TrainingSpeed);

    public void TrainPerception(double amount, Session session)
        => Attributes.Perception.Train(amount, Genus.PerceptionTraining, session.TrainingSpeed);

    public void TrainIntelligence(double amount, Session session)
        => Attributes.Intelligence.Train(amount, Genus.IntelligenceTraining, session.TrainingSpeed);

    public void TrainWillpower(double amount, Session session)
        => Attributes.Willpower.Train(amount, Genus.WillpowerTraining, session.TrainingSpeed);

    public void TrainCharisma(double amount, Session session)
        => Attributes.Charisma.Train(amount, Genus.CharismaTraining, session.TrainingSpeed);

    // ========== Depth-Based Training ==========

    /// <summary>
    /// Trains attributes based on dungeon depth. Uses Genus TrainingFunctions.
    /// At MinDepth: no training. Each level deeper adds 0.01-0.03 training per attribute.
    /// Override in subclasses for custom behavior (e.g., Player with professions).
    /// </summary>
    public virtual void DepthTraining(int depth, Random random)
    {
        int depthDelta = Math.Max(0, depth - MinDepth);
        if (depthDelta == 0) return;

        double minAmount = depthDelta * 0.01;
        double maxAmount = minAmount + 0.02;

        TrainAttrForDepth(Attributes.Strength, Genus.StrengthTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Endurance, Genus.EnduranceTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Agility, Genus.AgilityTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Perception, Genus.PerceptionTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Intelligence, Genus.IntelligenceTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Willpower, Genus.WillpowerTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Charisma, Genus.CharismaTraining, minAmount, maxAmount, random);
    }

    private void TrainAttrForDepth(Erg.Core.Systems.Attribute attr, TrainingFunction func, double min, double max, Random random)
    {
        double amount = min + random.NextDouble() * (max - min);
        attr.Train(amount, func, 1.0);
    }

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
        Dice? unarmedDamage = null,
        IBehavior? behavior = null)
        : base(name, x, y, character, fg, bg)
    {
        Energy = 0;
        Attributes = new Attributes();
        HitPoints = MaxHitPoints;
        UnarmedDamage = unarmedDamage ?? new Dice(1, 4);
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

    // Pełne uleczenie
    public void FullHeal()
    {
        HitPoints = MaxHitPoints;
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
}