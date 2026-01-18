namespace Erg.Core.Systems;

/// <summary>
/// Collection of all basic skills for a critter.
/// Each skill ranges from 0 to 1 and represents trained abilities.
/// </summary>
public class Skills
{
    /// <summary>Ability to read and comprehend written text.</summary>
    public Skill Reading { get; }

    /// <summary>Ability to swim and move through water.</summary>
    public Skill Swimming { get; }

    /// <summary>Proficiency in unarmed combat (punching, kicking, grappling).</summary>
    public Skill Unarmed { get; }

    /// <summary>
    /// Creates skills with all values set to the same default.
    /// </summary>
    public Skills(double defaultValue = 0.0)
    {
        Reading = new Skill(defaultValue);
        Swimming = new Skill(defaultValue);
        Unarmed = new Skill(defaultValue);
    }

    /// <summary>
    /// Creates skills with individual values.
    /// </summary>
    public Skills(double reading, double swimming, double unarmed)
    {
        Reading = new Skill(reading);
        Swimming = new Skill(swimming);
        Unarmed = new Skill(unarmed);
    }

    /// <summary>
    /// Enumerates all skills for iteration.
    /// </summary>
    public IEnumerable<(string Name, Skill Skill)> All()
    {
        yield return ("Reading", Reading);
        yield return ("Swimming", Swimming);
        yield return ("Unarmed", Unarmed);
    }

    /// <summary>
    /// Clears all modifiers from all skills.
    /// </summary>
    public void ClearAllModifiers()
    {
        foreach (var (_, skill) in All())
        {
            skill.ClearModifiers();
        }
    }
}
