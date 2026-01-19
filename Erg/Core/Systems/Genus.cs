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
    /// Speed = Lerp(EnergyRegenRateMin, EnergyRegenRateMax, Agility)
    /// </summary>
    public int EnergyRegenRateMin { get; init; } = 50;
    public int EnergyRegenRateMax { get; init; } = 150;

    /// <summary>
    /// Hit points range for the genus. Actual HitPointsMax is calculated as:
    /// HitPointsMax = Lerp(HitPointsMin, HitPointsMax, Endurance)
    /// </summary>
    public int HitPointsMin { get; init; } = 10;
    public int HitPointsMax { get; init; } = 30;

    /// <summary>
    /// Damage bonus range for the genus. Actual DamageBonus is calculated as:
    /// DamageBonus = Lerp(DamageBonusMin, DamageBonusMax, Speed)
    /// Represents kinetic energy of strikes - faster creatures hit harder.
    /// </summary>
    public int DamageBonusMin { get; init; } = 0;
    public int DamageBonusMax { get; init; } = 5;

    // ========== Ability Ranges (Unarmed Combat) ==========

    /// <summary>
    /// Unarmed attack ability range. Actual value is:
    /// UnarmedAttack = Lerp(Min, Max, UnarmedAttackProficiency)
    /// </summary>
    public int UnarmedAttackMin { get; init; } = 10;
    public int UnarmedAttackMax { get; init; } = 50;

    /// <summary>
    /// Unarmed defense ability range. Actual value is:
    /// UnarmedDefense = Lerp(Min, Max, UnarmedDefenseProficiency)
    /// </summary>
    public int UnarmedDefenseMin { get; init; } = 10;
    public int UnarmedDefenseMax { get; init; } = 50;

    // ========== Searching Ability ==========

    /// <summary>
    /// Searching ability range (0-100%). Higher is better.
    /// Actual value: Lerp(SearchingMin, SearchingMax, Observation)
    /// </summary>
    public int SearchingMin { get; init; } = 10;
    public int SearchingMax { get; init; } = 80;

    // ========== Reading Ability ==========

    /// <summary>
    /// Reading ability range (0-100%). Actual value:
    /// Reading = Lerp(ReadingMin, ReadingMax, LiteracyProficiency)
    /// Affects how much training is gained from reading books.
    /// </summary>
    public int ReadingMin { get; init; } = 0;
    public int ReadingMax { get; init; } = 100;

    // ========== Swimming Ability ==========

    /// <summary>
    /// Swimming ability range (0-100). Actual value:
    /// Swimming = Lerp(SwimmingMin, SwimmingMax, SwimmingSkill)
    /// Affects movement cost in water: 0=2x slower, 50=neutral, 100=2x faster.
    /// </summary>
    public int SwimmingMin { get; init; } = 0;
    public int SwimmingMax { get; init; } = 100;

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
    public TrainingFunction LiteracyTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction SwimmingTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction UnarmedTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction AttackTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction DefenseTraining { get; init; } = TrainingFunction.Linear();

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
        if (ReferenceEquals(skill, skills.LiteracySkill)) return LiteracyTraining;
        if (ReferenceEquals(skill, skills.SwimmingSkill)) return SwimmingTraining;
        if (ReferenceEquals(skill, skills.UnarmedSkill)) return UnarmedTraining;
        if (ReferenceEquals(skill, skills.AttackSkill)) return AttackTraining;
        if (ReferenceEquals(skill, skills.DefenseSkill)) return DefenseTraining;

        // Fallback to linear if skill not recognized
        return TrainingFunction.Linear();
    }

    // ========== Predefiniowane rodzaje ==========

    /// <summary>
    /// Człowiek — przeciętniak w każdym calu.
    /// </summary>
    public static Genus Human { get; } = new("Human")
    {
        EnergyRegenRateMin = 80, EnergyRegenRateMax = 120,
        HitPointsMin = 60, HitPointsMax = 100,
        DamageBonusMin = 2, DamageBonusMax = 12,
        UnarmedAttackMin = 10, UnarmedAttackMax = 50,
        UnarmedDefenseMin = 10, UnarmedDefenseMax = 50,
        SearchingMin = 10, SearchingMax = 90,
        ReadingMin = 0, ReadingMax = 100,
        SwimmingMin = 0, SwimmingMax = 100
    };

    /// <summary>
    /// Troll — silny fizycznie, powolny.
    /// </summary>
    public static Genus Troll { get; } = new("Troll")
    {
        EnergyRegenRateMin = 60, EnergyRegenRateMax = 90,
        HitPointsMin = 130, HitPointsMax = 240,
        DamageBonusMin = 6, DamageBonusMax = 18,
        UnarmedAttackMin = 25, UnarmedAttackMax = 80,
        UnarmedDefenseMin = 15, UnarmedDefenseMax = 40,
        SearchingMin = 10, SearchingMax = 50,
        ReadingMin = 0, ReadingMax = 30,
        SwimmingMin = 0, SwimmingMax = 40
    };

    /// <summary>
    /// Jelly — galaretowate stworzenia, bardzo wolne i wrażliwe na ciosy, preferują wodę.
    /// </summary>
    public static Genus Jelly { get; } = new("Jelly")
    {
        EnergyRegenRateMin = 30, EnergyRegenRateMax = 70,
        HitPointsMin = 10, HitPointsMax = 35,
        DamageBonusMin = 0, DamageBonusMax = 3,
        UnarmedAttackMin = 5, UnarmedAttackMax = 20,
        UnarmedDefenseMin = 0, UnarmedDefenseMax = 10,
        SearchingMin = 5, SearchingMax = 30,
        ReadingMin = 0, ReadingMax = 0,
        SwimmingMin = 60, SwimmingMax = 100
    };

    /// <summary>
    /// Construct — sztuczne konstrukty. Żywotne, trochę wolniejsze od człowieka.
    /// </summary>
    public static Genus Construct { get; } = new("Construct")
    {
        EnergyRegenRateMin = 70, EnergyRegenRateMax = 110,
        HitPointsMin = 90, HitPointsMax = 130,
        DamageBonusMin = 2, DamageBonusMax = 16,
        UnarmedAttackMin = 15, UnarmedAttackMax = 60,
        UnarmedDefenseMin = 0, UnarmedDefenseMax = 10,
        SearchingMin = 20, SearchingMax = 70,
        ReadingMin = 0, ReadingMax = 15,
        SwimmingMin = 0, SwimmingMax = 20
    };

    /// <summary>
    /// Zombius — zombie, utopce, szubieniczniki. Niezbyt nadgryzione czasem, chodzące trupy.
    /// </summary>
    public static Genus Zombius { get; } = new("Zombius")
    {
        EnergyRegenRateMin = 60, EnergyRegenRateMax = 90,
        HitPointsMin = 40, HitPointsMax = 120,
        DamageBonusMin = 1, DamageBonusMax = 6,
        UnarmedAttackMin = 15, UnarmedAttackMax = 40,
        UnarmedDefenseMin = 0, UnarmedDefenseMax = 10,
        SearchingMin = 5, SearchingMax = 30,
        ReadingMin = 0, ReadingMax = 5,
        SwimmingMin = 0, SwimmingMax = 70
    };
}
