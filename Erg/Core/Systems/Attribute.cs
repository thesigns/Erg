namespace Erg.Core.Systems;

/// <summary>
/// Represents a single attribute (e.g., Strength, Agility) with base value and modifiers.
/// Values are normalized to 0-1 range.
/// </summary>
public class Attribute
{
    private double _baseValue;
    private readonly Dictionary<object, double> _modifiers = new();

    /// <summary>
    /// Base (permanent) value of the attribute, developed through training. Range: 0-1.
    /// </summary>
    public double BaseValue
    {
        get => _baseValue;
        private set => _baseValue = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>
    /// Sum of all active modifiers from various sources (items, effects, etc.).
    /// </summary>
    public double TotalModifier => _modifiers.Values.Sum();

    /// <summary>
    /// Current effective value (BaseValue + modifiers), clamped to 0-1.
    /// </summary>
    public double CurrentValue => Math.Clamp(BaseValue + TotalModifier, 0.0, 1.0);

    /// <summary>
    /// Wartość do wyświetlenia w GUI (1-99).
    /// 1 = minimum (0.0), 99 = maksimum (1.0)
    /// </summary>
    public string DisplayValue => ((int)(1 + CurrentValue * 98)).ToString("D2");

    /// <summary>
    /// Training factor based on current base value. Higher values train slower.
    /// Formula: 1 - BaseValue (default, can be overridden by TrainingFunction)
    /// </summary>
    public double TrainingFactor => 1.0 - BaseValue;

    public Attribute(double baseValue = 0.5)
    {
        BaseValue = baseValue;
    }

    /// <summary>
    /// Sets the base value directly. Use for creature initialization.
    /// </summary>
    public void SetBaseValue(double value)
    {
        BaseValue = value;
    }

    /// <summary>
    /// Trains the attribute using a specific training function.
    /// Positive amount = training (increase), negative = atrophy (decrease).
    /// Change is: amount * function.Calculate(BaseValue) * trainingSpeed
    /// </summary>
    public void Train(double amount, TrainingFunction function, double trainingSpeed)
    {
        double factor = function.Calculate(BaseValue);
        BaseValue += amount * factor * trainingSpeed;
    }

    /// <summary>
    /// Trains the attribute using the default linear function.
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
