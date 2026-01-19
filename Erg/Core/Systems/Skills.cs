namespace Erg.Core.Systems;

/// <summary>
/// Collection of all basic skills for a critter.
/// Each skill ranges from 0 to 1 and represents trained skills.
/// </summary>
public class Skills
{
    /// <summary>Skill in reading and comprehending written text.</summary>
    public Skill LiteracySkill { get; }

    /// <summary>Skill in swimming and moving through water.</summary>
    public Skill SwimmingSkill { get; }

    /// <summary>Skill in unarmed combat (punching, kicking, grappling).</summary>
    public Skill UnarmedSkill { get; }

    /// <summary>Skill in attacking - hitting, aiming, timing.</summary>
    public Skill AttackSkill { get; }

    /// <summary>Skill in defending - dodging, blocking, parrying.</summary>
    public Skill DefenseSkill { get; }

    /// <summary>
    /// Creates skills with all values set to the same default.
    /// </summary>
    public Skills(double defaultValue = 0.0)
    {
        LiteracySkill = new Skill(defaultValue);
        SwimmingSkill = new Skill(defaultValue);
        UnarmedSkill = new Skill(defaultValue);
        AttackSkill = new Skill(defaultValue);
        DefenseSkill = new Skill(defaultValue);
    }

    /// <summary>
    /// Creates skills with individual values.
    /// </summary>
    public Skills(double literacy, double swimming, double unarmed, double attack = 0.0, double defense = 0.0)
    {
        LiteracySkill = new Skill(literacy);
        SwimmingSkill = new Skill(swimming);
        UnarmedSkill = new Skill(unarmed);
        AttackSkill = new Skill(attack);
        DefenseSkill = new Skill(defense);
    }

    /// <summary>
    /// Enumerates all skills for iteration.
    /// </summary>
    public IEnumerable<(string Name, Skill Skill)> All()
    {
        yield return ("Literacy", LiteracySkill);
        yield return ("Swimming", SwimmingSkill);
        yield return ("Unarmed", UnarmedSkill);
        yield return ("Attack", AttackSkill);
        yield return ("Defense", DefenseSkill);
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
