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
    /// Speed = Lerp(MinSpeed, MaxSpeed, Agility)
    /// </summary>
    public int MinSpeed { get; init; } = 50;
    public int MaxSpeed { get; init; } = 150;

    /// <summary>
    /// Hit points range for the genus. Actual MaxHitPoints is calculated as:
    /// MaxHitPoints = Lerp(MinHitPoints, MaxHitPoints, Endurance)
    /// </summary>
    public int MinHitPoints { get; init; } = 10;
    public int MaxHitPoints { get; init; } = 30;

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
        MinSpeed = 80, MaxSpeed = 120,
        MinHitPoints = 60, MaxHitPoints = 100
    };

    /// <summary>
    /// Troll — silny fizycznie, powolny.
    /// </summary>
    public static Genus Troll { get; } = new("Troll")
    {
        MinSpeed = 60, MaxSpeed = 90,
        MinHitPoints = 130, MaxHitPoints = 240
    };

    /// <summary>
    /// Jelly — galaretowate stworzenia, bardzo wolne i wrażliwe na ciosy, preferują wodę.
    /// </summary>
    public static Genus Jelly { get; } = new("Jelly")
    {
        MinSpeed = 30, MaxSpeed = 70,
        MinHitPoints = 10, MaxHitPoints = 35
    };

    /// <summary>
    /// Construct — sztuczne konstrukty. Żywotne, trochę wolniejsze od człowieka.
    /// </summary>
    public static Genus Construct { get; } = new("Construct")
    {
        MinSpeed = 70, MaxSpeed = 110,
        MinHitPoints = 90, MaxHitPoints = 130
    };

    /// <summary>
    /// Risen — ożywieńcy (zombie). Powolne, ale trudniejsze do zabicia.
    /// </summary>
    public static Genus Risen { get; } = new("Risen")
    {
        MinSpeed = 70, MaxSpeed = 90,
        MinHitPoints = 90, MaxHitPoints = 120
    };
}
