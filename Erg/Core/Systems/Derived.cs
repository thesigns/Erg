namespace Erg.Core.Systems;

/// <summary>
/// Wartości pochodne - obliczane na podstawie atrybutów podstawowych i umiejętności.
/// Nie są rozwijane bezpośrednio, zmieniają się gdy zmieniają się atrybuty/umiejętności źródłowe.
/// </summary>
public class Derived
{
    private readonly Attributes _attributes;
    private readonly Skills _skills;

    public Derived(Attributes attributes, Skills skills)
    {
        _attributes = attributes;
        _skills = skills;
    }

    // ========== Derived Attributes ==========

    /// <summary>
    /// Ogólna kondycja organizmu, odporność biologiczna, zdolność do regeneracji.
    /// Używane do obliczania MaxHitPoints, tempa regeneracji itp.
    /// </summary>
    public double Vitality =>
        0.2 * _attributes.Strength.CurrentValue +
        0.6 * _attributes.Endurance.CurrentValue +
        0.1 * _attributes.Agility.CurrentValue +
        0.1 * _attributes.Willpower.CurrentValue;

    /// <summary>
    /// Eksplozywna szybkość - zdolność do szybkiego działania.
    /// Używane do obliczania EnergyRegenRate.
    /// </summary>
    public double Speed =>
        0.4 * _attributes.Strength.CurrentValue +
        0.4 * _attributes.Agility.CurrentValue +
        0.1 * _attributes.Endurance.CurrentValue +
        0.1 * _attributes.Willpower.CurrentValue;

    // ========== Derived Skills ==========

    /// <summary>
    /// Pochodna umiejętność ataku wręcz - kombinacja szybkości i treningu walki.
    /// Używane do obliczania UnarmedAttack ability.
    /// </summary>
    public double UnarmedAttackSkill =>
        0.2 * Speed +
        0.6 * _skills.UnarmedSkill.CurrentValue +
        0.2 * _skills.AttackSkill.CurrentValue;

    /// <summary>
    /// Pochodna umiejętność obrony wręcz - kombinacja szybkości i treningu walki.
    /// Używane do obliczania UnarmedDefense ability.
    /// </summary>
    public double UnarmedDefenseSkill =>
        0.2 * Speed +
        0.6 * _skills.UnarmedSkill.CurrentValue +
        0.2 * _skills.DefenseSkill.CurrentValue;

    // ========== Display Values (0-100) ==========

    private static string ToDisplayValue(double value) => ((int)(value * 100)).ToString();

    public string VitalityDisplay => ToDisplayValue(Vitality);
    public string SpeedDisplay => ToDisplayValue(Speed);
    public string UnarmedAttackSkillDisplay => ToDisplayValue(UnarmedAttackSkill);
    public string UnarmedDefenseSkillDisplay => ToDisplayValue(UnarmedDefenseSkill);
}
