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
    /// Energy regeneration rate interpolated from Genus min/max based on Speed.
    /// Formula: Lerp(MinEnergyRegenRate, MaxEnergyRegenRate, Derived.Speed)
    /// </summary>
    public int EnergyRegenRate => Lerp(Genus.MinEnergyRegenRate, Genus.MaxEnergyRegenRate, Derived.Speed);
    public int Energy { get; protected set; }  // akumulowana
    public Inventory Inventory { get; } = new();
    public Attributes Attributes { get; }
    public Skills Skills { get; }
    public Derived Derived { get; }
    public Genus Genus { get; protected set; } = Genus.Human;
    public IBehavior? Behavior { get; protected set; }

    // Hit Points
    /// <summary>
    /// Max HP interpolated from Genus.MinHitPoints to Genus.MaxHitPoints based on Vitality.
    /// Formula: Lerp(MinHitPoints, MaxHitPoints, Vitality)
    /// </summary>
    public int MaxHitPoints => Lerp(Genus.MinHitPoints, Genus.MaxHitPoints, Derived.Vitality);
    public int HitPoints { get; protected set; }
    public bool IsAlive => HitPoints > 0;

    private static int Lerp(int min, int max, double t)
    {
        return min + (int)((max - min) * t);
    }

    // Combat
    public Dice UnarmedDamage { get; protected set; }
    /// <summary>
    /// Bonus damage based on Speed. Represents kinetic energy of strikes.
    /// Formula: Lerp(MinDamageBonus, MaxDamageBonus, Speed)
    /// </summary>
    public int DamageBonus => Lerp(Genus.MinDamageBonus, Genus.MaxDamageBonus, Derived.Speed);

    // ========== Unarmed Combat Abilities ==========
    /// <summary>
    /// Unarmed attack ability. Used in combat to determine hit chance.
    /// Formula: Lerp(UnarmedAttackMin, UnarmedAttackMax, UnarmedAttackProficiency)
    /// </summary>
    public int UnarmedAttack => Lerp(Genus.UnarmedAttackMin, Genus.UnarmedAttackMax, Derived.UnarmedAttackProficiency);

    /// <summary>
    /// Unarmed defense ability. Used in combat to determine dodge/block chance.
    /// Formula: Lerp(UnarmedDefenseMin, UnarmedDefenseMax, UnarmedDefenseProficiency)
    /// </summary>
    public int UnarmedDefense => Lerp(Genus.UnarmedDefenseMin, Genus.UnarmedDefenseMax, Derived.UnarmedDefenseProficiency);

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
        return LineOfSight.CanSee(area, X, Y, x, y, SightRange);
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

    // ========== Skill Training ==========

    /// <summary>
    /// Trains a skill using the species-specific training function.
    /// </summary>
    public void TrainSkill(Skill skill, double amount, Session session)
    {
        var function = Genus.GetSkillTrainingFunction(skill, Skills);
        skill.Train(amount, function, session.TrainingSpeed);
    }

    public void TrainReading(double amount, Session session)
        => Skills.ReadingSkill.Train(amount, Genus.ReadingTraining, session.TrainingSpeed);

    public void TrainSwimming(double amount, Session session)
        => Skills.SwimmingSkill.Train(amount, Genus.SwimmingTraining, session.TrainingSpeed);

    public void TrainUnarmed(double amount, Session session)
        => Skills.UnarmedSkill.Train(amount, Genus.UnarmedTraining, session.TrainingSpeed);

    public void TrainAttack(double amount, Session session)
        => Skills.AttackSkill.Train(amount, Genus.AttackTraining, session.TrainingSpeed);

    public void TrainDefense(double amount, Session session)
        => Skills.DefenseSkill.Train(amount, Genus.DefenseTraining, session.TrainingSpeed);

    // ========== Depth-Based Training ==========

    /// <summary>
    /// Trains attributes based on dungeon depth. Uses Genus TrainingFunctions.
    /// Base training: 0.2-0.5, plus depth bonus: 0.02-0.05 per level above MinDepth.
    /// Override in subclasses for custom behavior (e.g., Player with professions).
    /// </summary>
    public virtual void DepthTraining(int depth, Random random)
    {
        int depthDelta = Math.Max(0, depth - MinDepth);

        // Bazowy trening: 0.2-0.5 (losowo)
        // Plus depth bonus: 0.02-0.05 per depth level
        double baseMin = 0.2;
        double baseMax = 0.5;
        double depthMin = depthDelta * 0.02;
        double depthMax = depthDelta * 0.05;

        double minAmount = baseMin + depthMin;
        double maxAmount = baseMax + depthMax;

        TrainAttrForDepth(Attributes.Strength, Genus.StrengthTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Endurance, Genus.EnduranceTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Agility, Genus.AgilityTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Perception, Genus.PerceptionTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Intelligence, Genus.IntelligenceTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Willpower, Genus.WillpowerTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Charisma, Genus.CharismaTraining, minAmount, maxAmount, random);
    }

    protected void TrainAttrForDepth(Erg.Core.Systems.Attribute attr, TrainingFunction func, double min, double max, Random random)
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
        Skills = new Skills();
        Derived = new Derived(Attributes, Skills);
        HitPoints = MaxHitPoints;
        UnarmedDamage = unarmedDamage ?? new Dice(1, 4);
        Behavior = behavior;
    }

    public void GainEnergy()
    {
        Energy += EnergyRegenRate;
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