namespace Erg.Core.Systems;

/// <summary>
/// Defines genus-level characteristics including training functions for each attribute.
/// Genus is a broad category (e.g., Human, Troll, Risen) while Critter subclasses
/// represent specific species (e.g., Zombie, Amoeba).
/// </summary>
public class Genus
{
    public string Name { get; }

    /// <summary>
    /// Speed range for the genus. Actual speed is calculated as:
    /// Speed = Lerp(MinEnergyRegenRate, MaxEnergyRegenRate, Agility)
    /// </summary>
    public int MinEnergyRegenRate { get; init; } = 50;
    public int MaxEnergyRegenRate { get; init; } = 150;

    /// <summary>
    /// Hit points range for the genus. Actual MaxHitPoints is calculated as:
    /// MaxHitPoints = Lerp(MinHitPoints, MaxHitPoints, Endurance)
    /// </summary>
    public int MinHitPoints { get; init; } = 10;
    public int MaxHitPoints { get; init; } = 30;

    /// <summary>
    /// Damage bonus range for the genus. Actual DamageBonus is calculated as:
    /// DamageBonus = Lerp(MinDamageBonus, MaxDamageBonus, Speed)
    /// Represents kinetic energy of strikes - faster creatures hit harder.
    /// </summary>
    public int MinDamageBonus { get; init; } = 0;
    public int MaxDamageBonus { get; init; } = 5;

    // ========== Ability Ranges (Unarmed Combat) ==========

    /// <summary>
    /// Unarmed attack ability range. Actual value is:
    /// UnarmedAttack = Lerp(Min, Max, UnarmedProficiency)
    /// </summary>
    public int UnarmedAttackMin { get; init; } = 10;
    public int UnarmedAttackMax { get; init; } = 50;

    /// <summary>
    /// Unarmed defense ability range. Actual value is:
    /// UnarmedDefense = Lerp(Min, Max, UnarmedProficiency)
    /// </summary>
    public int UnarmedDefenseMin { get; init; } = 10;
    public int UnarmedDefenseMax { get; init; } = 50;

    // ========== Attribute Training Functions ==========
    // Funkcje treningowe per atrybut (domyślnie Linear)
    public TrainingFunction StrengthTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction EnduranceTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction AgilityTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction PerceptionTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction IntelligenceTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction WillpowerTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction CharismaTraining { get; init; } = TrainingFunction.Linear();

    // ========== Skill Training Functions ==========
    // Funkcje treningowe per umiejętność (domyślnie Linear)
    public TrainingFunction ReadingTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction SwimmingTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction UnarmedTraining { get; init; } = TrainingFunction.Linear();

    public Genus(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the training function for a specific attribute.
    /// </summary>
    public TrainingFunction GetTrainingFunction(Attribute attr, Attributes attrs)
    {
        if (ReferenceEquals(attr, attrs.Strength)) return StrengthTraining;
        if (ReferenceEquals(attr, attrs.Endurance)) return EnduranceTraining;
        if (ReferenceEquals(attr, attrs.Agility)) return AgilityTraining;
        if (ReferenceEquals(attr, attrs.Perception)) return PerceptionTraining;
        if (ReferenceEquals(attr, attrs.Intelligence)) return IntelligenceTraining;
        if (ReferenceEquals(attr, attrs.Willpower)) return WillpowerTraining;
        if (ReferenceEquals(attr, attrs.Charisma)) return CharismaTraining;

        // Fallback to linear if attribute not recognized
        return TrainingFunction.Linear();
    }

    /// <summary>
    /// Gets the training function for a specific skill.
    /// </summary>
    public TrainingFunction GetSkillTrainingFunction(Skill skill, Skills skills)
    {
        if (ReferenceEquals(skill, skills.Reading)) return ReadingTraining;
        if (ReferenceEquals(skill, skills.Swimming)) return SwimmingTraining;
        if (ReferenceEquals(skill, skills.Unarmed)) return UnarmedTraining;

        // Fallback to linear if skill not recognized
        return TrainingFunction.Linear();
    }

    // ========== Predefiniowane rodzaje ==========

    /// <summary>
    /// Człowiek — przeciętniak w każdym calu.
    /// </summary>
    public static Genus Human { get; } = new("Human")
    {
        MinEnergyRegenRate = 80, MaxEnergyRegenRate = 120,
        MinHitPoints = 60, MaxHitPoints = 100,
        MinDamageBonus = 2, MaxDamageBonus = 12,
        UnarmedAttackMin = 10, UnarmedAttackMax = 50,
        UnarmedDefenseMin = 10, UnarmedDefenseMax = 50
    };

    /// <summary>
    /// Troll — silny fizycznie, powolny.
    /// </summary>
    public static Genus Troll { get; } = new("Troll")
    {
        MinEnergyRegenRate = 60, MaxEnergyRegenRate = 90,
        MinHitPoints = 130, MaxHitPoints = 240,
        MinDamageBonus = 6, MaxDamageBonus = 18,
        UnarmedAttackMin = 25, UnarmedAttackMax = 80,
        UnarmedDefenseMin = 15, UnarmedDefenseMax = 40
    };

    /// <summary>
    /// Jelly — galaretowate stworzenia, bardzo wolne i wrażliwe na ciosy, preferują wodę.
    /// </summary>
    public static Genus Jelly { get; } = new("Jelly")
    {
        MinEnergyRegenRate = 30, MaxEnergyRegenRate = 70,
        MinHitPoints = 10, MaxHitPoints = 35,
        MinDamageBonus = 0, MaxDamageBonus = 3,
        UnarmedAttackMin = 5, UnarmedAttackMax = 20,
        UnarmedDefenseMin = 0, UnarmedDefenseMax = 10
    };

    /// <summary>
    /// Construct — sztuczne konstrukty. Żywotne, trochę wolniejsze od człowieka.
    /// </summary>
    public static Genus Construct { get; } = new("Construct")
    {
        MinEnergyRegenRate = 70, MaxEnergyRegenRate = 110,
        MinHitPoints = 90, MaxHitPoints = 130,
        MinDamageBonus = 2, MaxDamageBonus = 16,
        UnarmedAttackMin = 15, UnarmedAttackMax = 60,
        UnarmedDefenseMin = 0, UnarmedDefenseMax = 10
    };

    /// <summary>
    /// Zombius — zombie, utopce, szubieniczniki. Niezbyt nadgryzione czasem, chodzące trupy.
    /// </summary>
    public static Genus Zombius { get; } = new("Zombius")
    {
        MinEnergyRegenRate = 60, MaxEnergyRegenRate = 90,
        MinHitPoints = 40, MaxHitPoints = 120,
        MinDamageBonus = 1, MaxDamageBonus = 6,
        UnarmedAttackMin = 15, UnarmedAttackMax = 40,
        UnarmedDefenseMin = 0, UnarmedDefenseMax = 10
    };
}
