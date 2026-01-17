namespace Erg.Core.Systems;

/// <summary>
/// Defines species-specific characteristics including training functions for each attribute.
/// </summary>
public class Species
{
    public string Name { get; }

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
        StrengthTraining = TrainingFunction.Capped(0.2),      // Siła łatwa do trenowania
        EnduranceTraining = TrainingFunction.Capped(0.15),    // Wytrzymałość też
        IntelligenceTraining = TrainingFunction.Quadratic(),  // Inteligencja bardzo trudna
        CharismaTraining = TrainingFunction.Quadratic()       // Charyzma też trudna
    };
}
