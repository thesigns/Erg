namespace Erg.Core.Systems;

/// <summary>
/// Collection of all basic skills for a critter.
/// Each skill ranges from 0 to 1 and represents trained abilities.
/// </summary>
public class Skills
{
    /// <summary>Ability to read and comprehend written text.</summary>
    public Skill LiteracySkill { get; }

    /// <summary>Ability to swim and move through water.</summary>
    public Skill SwimmingSkill { get; }

    /// <summary>Proficiency in unarmed combat (punching, kicking, grappling).</summary>
    public Skill UnarmedSkill { get; }

    /// <summary>General attack proficiency - hitting, aiming, timing.</summary>
    public Skill AttackSkill { get; }

    /// <summary>General defense proficiency - dodging, blocking, parrying.</summary>
    public Skill DefenseSkill { get; }

    /// <summary>
    /// Creates skills with all values set to the same default.
    /// Each skill has configured theory/practice limits based on its nature.
    /// </summary>
    public Skills(double defaultValue = 0.0)
    {
        // Literacy: 40% theory, 60% practice (reading itself is practice)
        LiteracySkill = new Skill(defaultValue) { TheoryMax = 0.4, PracticeMax = 0.6 };

        // Swimming: 20% theory, 80% practice (can't learn to swim from books alone)
        SwimmingSkill = new Skill(defaultValue) { TheoryMax = 0.2, PracticeMax = 0.8 };

        // Combat skills: 30% theory, 70% practice (fighting requires real experience)
        UnarmedSkill = new Skill(defaultValue) { TheoryMax = 0.3, PracticeMax = 0.7 };
        AttackSkill = new Skill(defaultValue) { TheoryMax = 0.3, PracticeMax = 0.7 };
        DefenseSkill = new Skill(defaultValue) { TheoryMax = 0.3, PracticeMax = 0.7 };
    }

    /// <summary>
    /// Creates skills with individual values.
    /// Each skill has configured theory/practice limits based on its nature.
    /// </summary>
    public Skills(double literacy, double swimming, double unarmed, double attack = 0.0, double defense = 0.0)
    {
        LiteracySkill = new Skill(literacy) { TheoryMax = 0.4, PracticeMax = 0.6 };
        SwimmingSkill = new Skill(swimming) { TheoryMax = 0.2, PracticeMax = 0.8 };
        UnarmedSkill = new Skill(unarmed) { TheoryMax = 0.3, PracticeMax = 0.7 };
        AttackSkill = new Skill(attack) { TheoryMax = 0.3, PracticeMax = 0.7 };
        DefenseSkill = new Skill(defense) { TheoryMax = 0.3, PracticeMax = 0.7 };
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
