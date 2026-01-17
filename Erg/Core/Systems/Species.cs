namespace Erg.Core.Systems;

/// <summary>
/// Defines species-specific characteristics including training functions for each attribute.
/// </summary>
public class Species
{
    public string Name { get; }

    /// <summary>
    /// Base speed for the species. Actual speed is calculated as:
    /// Speed = BaseSpeed * (0.5 + Agility)
    /// </summary>
    public int BaseSpeed { get; init; } = 100;

    // Funkcje treningowe per atrybut (domyślnie Linear)
    public TrainingFunction StrengthTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction EnduranceTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction AgilityTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction PerceptionTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction IntelligenceTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction WillpowerTraining { get; init; } = TrainingFunction.Linear();
    public TrainingFunction CharismaTraining { get; init; } = TrainingFunction.Linear();

    public Species(string name)
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

    // ========== Predefiniowane gatunki ==========

    /// <summary>
    /// Człowiek — zbalansowane, standardowe krzywe treningowe.
    /// </summary>
    public static Species Human { get; } = new("Human");

    /// <summary>
    /// Troll — silny fizycznie, słabszy intelektualnie.
    /// </summary>
    public static Species Troll { get; } = new("Troll")
    {
        BaseSpeed = 80,
        StrengthTraining = TrainingFunction.Capped(0.2),      // Siła łatwa do trenowania
        EnduranceTraining = TrainingFunction.Capped(0.15),    // Wytrzymałość też
        IntelligenceTraining = TrainingFunction.Quadratic(),  // Inteligencja bardzo trudna
        CharismaTraining = TrainingFunction.Quadratic()       // Charyzma też trudna
    };

    /// <summary>
    /// Jelly — galaretowate stworzenia, wolniejsze, preferują wodę.
    /// </summary>
    public static Species Jelly { get; } = new("Jelly")
    {
        BaseSpeed = 90,
        AgilityTraining = TrainingFunction.Quadratic(),   // mniej zwinne
        EnduranceTraining = TrainingFunction.Capped(0.2)  // wytrzymałe
    };

    /// <summary>
    /// Construct — sztuczne konstrukty, nie trenują w tradycyjny sposób.
    /// </summary>
    public static Species Construct { get; } = new("Construct")
    {
        BaseSpeed = 80,
        StrengthTraining = TrainingFunction.Constant(0),
        EnduranceTraining = TrainingFunction.Constant(0),
        AgilityTraining = TrainingFunction.Constant(0),
        PerceptionTraining = TrainingFunction.Constant(0),
        IntelligenceTraining = TrainingFunction.Constant(0),
        WillpowerTraining = TrainingFunction.Constant(0),
        CharismaTraining = TrainingFunction.Constant(0)
    };

    /// <summary>
    /// Risen — ożywieńcy, nieumarli, powolny ale pewny progres.
    /// </summary>
    public static Species Risen { get; } = new("Risen")
    {
        BaseSpeed = 80,
        StrengthTraining = TrainingFunction.Capped(0.15),
        IntelligenceTraining = TrainingFunction.Quadratic(),
        CharismaTraining = TrainingFunction.Constant(0)  // brak charyzmy
    };
}
