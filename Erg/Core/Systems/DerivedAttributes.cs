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
}
