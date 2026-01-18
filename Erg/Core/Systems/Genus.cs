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

    // Funkcje treningowe per atrybut (domyślnie Linear)
    public TrainingFunction StrengthTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction EnduranceTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction AgilityTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction PerceptionTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction IntelligenceTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction WillpowerTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction CharismaTraining { get; init; } = TrainingFunction.Linear();

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

    // ========== Predefiniowane rodzaje ==========

    /// <summary>
    /// Człowiek — przeciętniak w każdym calu.
    /// </summary>
    public static Genus Human { get; } = new("Human")
    {
        MinEnergyRegenRate = 80, MaxEnergyRegenRate = 120,
        MinHitPoints = 60, MaxHitPoints = 100,
        MinDamageBonus = 2, MaxDamageBonus = 12
    };

    /// <summary>
    /// Troll — silny fizycznie, powolny.
    /// </summary>
    public static Genus Troll { get; } = new("Troll")
    {
        MinEnergyRegenRate = 60, MaxEnergyRegenRate = 90,
        MinHitPoints = 130, MaxHitPoints = 240,
        MinDamageBonus = 6, MaxDamageBonus = 18
    };

    /// <summary>
    /// Jelly — galaretowate stworzenia, bardzo wolne i wrażliwe na ciosy, preferują wodę.
    /// </summary>
    public static Genus Jelly { get; } = new("Jelly")
    {
        MinEnergyRegenRate = 30, MaxEnergyRegenRate = 70,
        MinHitPoints = 10, MaxHitPoints = 35,
        MinDamageBonus = 0, MaxDamageBonus = 3
    };

    /// <summary>
    /// Construct — sztuczne konstrukty. Żywotne, trochę wolniejsze od człowieka.
    /// </summary>
    public static Genus Construct { get; } = new("Construct")
    {
        MinEnergyRegenRate = 70, MaxEnergyRegenRate = 110,
        MinHitPoints = 90, MaxHitPoints = 130,
        MinDamageBonus = 2, MaxDamageBonus = 16
    };

    /// <summary>
    /// Zombius — zombie, utopce, szubieniczniki. Niezbyt nadgryzione czasem, chodzące trupy.
    /// </summary>
    public static Genus Zombius { get; } = new("Zombius")
    {
        MinEnergyRegenRate = 70, MaxEnergyRegenRate = 90,
        MinHitPoints = 20, MaxHitPoints = 120,
        MinDamageBonus = 1, MaxDamageBonus = 6
    };
}
