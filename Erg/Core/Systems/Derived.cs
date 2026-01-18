namespace Erg.Core.Systems;

/// <summary>
/// Derived values - calculated from base attributes and skills.
/// Not trained directly, they change when source attributes/skills change.
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
    /// Overall physical condition, biological resistance, regeneration capability.
    /// Used for calculating MaxHitPoints, regeneration rate, etc.
    /// </summary>
    public double Vitality =>
        0.2 * _attributes.Strength.CurrentValue +
        0.6 * _attributes.Endurance.CurrentValue +
        0.1 * _attributes.Agility.CurrentValue +
        0.1 * _attributes.Willpower.CurrentValue;

    /// <summary>
    /// Explosive speed - ability to act quickly.
    /// Used for calculating EnergyRegenRate.
    /// </summary>
    public double Speed =>
        0.4 * _attributes.Strength.CurrentValue +
        0.4 * _attributes.Agility.CurrentValue +
        0.1 * _attributes.Endurance.CurrentValue +
        0.1 * _attributes.Willpower.CurrentValue;

    // ========== Derived Skills ==========

    /// <summary>
    /// Derived unarmed attack skill - combination of speed and combat training.
    /// Used for calculating UnarmedAttack ability.
    /// </summary>
    public double UnarmedAttackSkill =>
        0.2 * Speed +
        0.6 * _skills.UnarmedSkill.CurrentValue +
        0.2 * _skills.AttackSkill.CurrentValue;

    /// <summary>
    /// Derived unarmed defense skill - combination of speed and combat training.
    /// Used for calculating UnarmedDefense ability.
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
