namespace Erg.Core.Systems;

/// <summary>
/// Collection of all basic attributes for a critter.
/// Each attribute ranges from 0 to 1 and represents innate characteristics.
/// </summary>
public class Attributes
{
    /// <summary>Physical power, carrying capacity, melee damage.</summary>
    public Attribute Strength { get; }

    /// <summary>Stamina, resistance to fatigue, hit points.</summary>
    public Attribute Endurance { get; }

    /// <summary>Speed, reflexes, fine motor control.</summary>
    public Attribute Agility { get; }

    /// <summary>Senses, awareness, ranged accuracy.</summary>
    public Attribute Perception { get; }

    /// <summary>Learning ability, memory, magical aptitude.</summary>
    public Attribute Intelligence { get; }

    /// <summary>Mental fortitude, resistance to fear and magic.</summary>
    public Attribute Willpower { get; }

    /// <summary>Social skills, leadership, persuasion.</summary>
    public Attribute Charisma { get; }

    /// <summary>
    /// Creates attributes with all values set to the same default.
    /// </summary>
    public Attributes(double defaultValue = 0.5)
    {
        Strength = new Attribute(defaultValue);
        Endurance = new Attribute(defaultValue);
        Agility = new Attribute(defaultValue);
        Perception = new Attribute(defaultValue);
        Intelligence = new Attribute(defaultValue);
        Willpower = new Attribute(defaultValue);
        Charisma = new Attribute(defaultValue);
    }

    /// <summary>
    /// Creates attributes with individual values.
    /// </summary>
    public Attributes(
        double strength,
        double endurance,
        double agility,
        double perception,
        double intelligence,
        double willpower,
        double charisma)
    {
        Strength = new Attribute(strength);
        Endurance = new Attribute(endurance);
        Agility = new Attribute(agility);
        Perception = new Attribute(perception);
        Intelligence = new Attribute(intelligence);
        Willpower = new Attribute(willpower);
        Charisma = new Attribute(charisma);
    }

    /// <summary>
    /// Enumerates all attributes for iteration.
    /// </summary>
    public IEnumerable<(string Name, Attribute Attribute)> All()
    {
        yield return ("Strength", Strength);
        yield return ("Endurance", Endurance);
        yield return ("Agility", Agility);
        yield return ("Perception", Perception);
        yield return ("Intelligence", Intelligence);
        yield return ("Willpower", Willpower);
        yield return ("Charisma", Charisma);
    }

    /// <summary>
    /// Clears all modifiers from all attributes.
    /// </summary>
    public void ClearAllModifiers()
    {
        foreach (var (_, attr) in All())
        {
            attr.ClearModifiers();
        }
    }
}
