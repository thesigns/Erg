namespace Erg.Core.Systems;

/// <summary>
/// Represents a single skill (e.g., Reading, Swimming, Unarmed) with theoretical and practical components.
/// Skills are split into theory (learned from books/study) and practice (learned from doing).
/// Values are normalized to 0-1 range.
/// </summary>
public class Skill
{
    private double _theoryValue;
    private double _practiceValue;
    private readonly Dictionary<object, double> _modifiers = new();

    /// <summary>
    /// Maximum value that can be achieved through theoretical training (books, study).
    /// Default is 0.5 (50% of skill from theory).
    /// </summary>
    public double TheoryMax { get; init; } = 0.5;

    /// <summary>
    /// Maximum value that can be achieved through practical training (doing the skill).
    /// Default is 0.5 (50% of skill from practice).
    /// </summary>
    public double PracticeMax { get; init; } = 0.5;

    /// <summary>
    /// Raw theoretical value (not clamped to TheoryMax).
    /// </summary>
    public double TheoryValue
    {
        get => _theoryValue;
        private set => _theoryValue = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>
    /// Raw practical value (not clamped to PracticeMax).
    /// </summary>
    public double PracticeValue
    {
        get => _practiceValue;
        private set => _practiceValue = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>
    /// Effective theoretical contribution (clamped to TheoryMax).
    /// </summary>
    public double EffectiveTheory => Math.Min(TheoryValue, TheoryMax);

    /// <summary>
    /// Effective practical contribution (clamped to PracticeMax).
    /// </summary>
    public double EffectivePractice => Math.Min(PracticeValue, PracticeMax);

    /// <summary>
    /// Base (permanent) value of the skill = EffectiveTheory + EffectivePractice, clamped to 0-1.
    /// </summary>
    public double BaseValue => Math.Clamp(EffectiveTheory + EffectivePractice, 0.0, 1.0);

    /// <summary>
    /// Sum of all active modifiers from various sources (items, effects, etc.).
    /// </summary>
    public double TotalModifier => _modifiers.Values.Sum();

    /// <summary>
    /// Current effective value (BaseValue + modifiers), clamped to 0-1.
    /// </summary>
    public double CurrentValue => Math.Clamp(BaseValue + TotalModifier, 0.0, 1.0);

    /// <summary>
    /// Display value for GUI (0-100).
    /// 0 = minimum (0.0), 100 = maximum (1.0)
    /// </summary>
    public string DisplayValue => ((int)(CurrentValue * 100)).ToString();

    /// <summary>
    /// Display value for theory component (0-100 based on EffectiveTheory).
    /// </summary>
    public string TheoryDisplayValue => ((int)(EffectiveTheory * 100)).ToString();

    /// <summary>
    /// Display value for practice component (0-100 based on EffectivePractice).
    /// </summary>
    public string PracticeDisplayValue => ((int)(EffectivePractice * 100)).ToString();

    public Skill(double baseValue = 0.0)
    {
        // For backward compatibility, split initial value equally between theory and practice
        TheoryValue = baseValue / 2;
        PracticeValue = baseValue / 2;
    }

    /// <summary>
    /// Sets the base value by splitting equally between theory and practice.
    /// Use for creature initialization when you don't care about the split.
    /// </summary>
    public void SetBaseValue(double value)
    {
        TheoryValue = value / 2;
        PracticeValue = value / 2;
    }

    /// <summary>
    /// Sets the theory value directly.
    /// </summary>
    public void SetTheoryValue(double value)
    {
        TheoryValue = value;
    }

    /// <summary>
    /// Sets the practice value directly.
    /// </summary>
    public void SetPracticeValue(double value)
    {
        PracticeValue = value;
    }

    /// <summary>
    /// Trains the theoretical component using a specific training function.
    /// Used for book learning and study.
    /// </summary>
    public void TrainTheory(double amount, TrainingFunction function, double trainingSpeed)
    {
        double factor = function.Calculate(TheoryValue);
        TheoryValue += amount * factor * trainingSpeed;
    }

    /// <summary>
    /// Trains the practical component using a specific training function.
    /// Used for learning by doing.
    /// </summary>
    public void TrainPractice(double amount, TrainingFunction function, double trainingSpeed)
    {
        double factor = function.Calculate(PracticeValue);
        PracticeValue += amount * factor * trainingSpeed;
    }

    /// <summary>
    /// Trains the skill (practice component) using a specific training function.
    /// Positive amount = training (increase), negative = atrophy (decrease).
    /// Change is: amount * function.Calculate(PracticeValue) * trainingSpeed
    /// </summary>
    public void Train(double amount, TrainingFunction function, double trainingSpeed)
    {
        // Default Train() trains practice for backward compatibility
        TrainPractice(amount, function, trainingSpeed);
    }

    /// <summary>
    /// Trains the skill (practice component) using the default linear function.
    /// Positive amount = training, negative = atrophy.
    /// </summary>
    public void Train(double amount, double trainingSpeed)
    {
        Train(amount, TrainingFunction.Linear(), trainingSpeed);
    }

    /// <summary>
    /// Sets a modifier from a specific source. Replaces any existing modifier from that source.
    /// </summary>
    public void SetModifier(object source, double value)
    {
        _modifiers[source] = value;
    }

    /// <summary>
    /// Removes a modifier from a specific source.
    /// </summary>
    public void RemoveModifier(object source)
    {
        _modifiers.Remove(source);
    }

    /// <summary>
    /// Checks if a modifier from the given source exists.
    /// </summary>
    public bool HasModifier(object source)
    {
        return _modifiers.ContainsKey(source);
    }

    /// <summary>
    /// Removes all modifiers.
    /// </summary>
    public void ClearModifiers()
    {
        _modifiers.Clear();
    }

    /// <summary>
    /// Gets the modifier value for a specific source, or 0 if not present.
    /// </summary>
    public double GetModifier(object source)
    {
        return _modifiers.TryGetValue(source, out var value) ? value : 0.0;
    }
}
