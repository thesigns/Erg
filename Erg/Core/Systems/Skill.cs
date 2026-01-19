namespace Erg.Core.Systems;

/// <summary>
/// Represents a single skill (e.g., Reading, Swimming, Unarmed).
/// Values are normalized to 0-1 range.
/// </summary>
public class Skill
{
    private double _value;
    private readonly Dictionary<object, double> _modifiers = new();

    /// <summary>
    /// Raw skill value (0-1).
    /// </summary>
    public double Value
    {
        get => _value;
        private set => _value = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>
    /// Base (permanent) value of the skill.
    /// </summary>
    public double BaseValue => Value;

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
    /// </summary>
    public string DisplayValue => ((int)(CurrentValue * 100)).ToString();

    public Skill(double baseValue = 0.0)
    {
        Value = baseValue;
    }

    /// <summary>
    /// Sets the base value directly.
    /// </summary>
    public void SetBaseValue(double value)
    {
        Value = value;
    }

    /// <summary>
    /// Trains the skill using a specific training function.
    /// Positive amount = training (increase), negative = atrophy (decrease).
    /// Change is: amount * function.Calculate(Value) * trainingSpeed
    /// </summary>
    public void Train(double amount, TrainingFunction function, double trainingSpeed)
    {
        double factor = function.Calculate(Value);
        Value += amount * factor * trainingSpeed;
    }

    /// <summary>
    /// Trains the skill using the default linear function.
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
