namespace Erg.Core.Systems;

/// <summary>
/// Atrybuty pochodne - wartości obliczane na podstawie atrybutów podstawowych.
/// Nie są rozwijane bezpośrednio, zmieniają się gdy zmieniają się atrybuty źródłowe.
/// </summary>
public class DerivedAttributes
{
    private readonly Attributes _attributes;

    public DerivedAttributes(Attributes attributes)
    {
        _attributes = attributes;
    }

    /// <summary>
    /// Ogólna kondycja organizmu, odporność biologiczna, zdolność do regeneracji.
    /// Używane do obliczania MaxHitPoints, tempa regeneracji itp.
    /// Formula: 0.2 * Strength + 0.5 * Endurance + 0.2 * Agility + 0.1 * Willpower
    /// </summary>
    public double Vitality =>
        0.2 * _attributes.Strength.CurrentValue +
        0.5 * _attributes.Endurance.CurrentValue +
        0.2 * _attributes.Agility.CurrentValue +
        0.1 * _attributes.Willpower.CurrentValue;

    /// <summary>
    /// Eksplozywna szybkość - zdolność do szybkiego działania.
    /// Używane do obliczania EnergyRegenRate.
    /// Formula: 0.4 * Strength + 0.4 * Agility + 0.1 * Endurance + 0.1 * Willpower
    /// </summary>
    public double Speed =>
        0.4 * _attributes.Strength.CurrentValue +
        0.4 * _attributes.Agility.CurrentValue +
        0.1 * _attributes.Endurance.CurrentValue +
        0.1 * _attributes.Willpower.CurrentValue;

    // ========== Display Values (1-99) ==========

    private static string ToDisplayValue(double value) => ((int)(1 + value * 98)).ToString("D2");

    public string VitalityDisplay => ToDisplayValue(Vitality);
    public string SpeedDisplay => ToDisplayValue(Speed);
}
