using Erg.Core.Systems;
using Erg.Core.Types;

namespace Erg.Core.World;

public class Player : Critter
{
    public override bool CanOpenDoor => true;

    public Player(int x, int y)
        : base("Player", x, y, '@', 0xFFFFFFFF, 0x000000FF)
    {
        // Reset attributes to 0.0 - DepthTraining will set them
        Attributes.Strength.SetBaseValue(0.0);
        Attributes.Endurance.SetBaseValue(0.0);
        Attributes.Agility.SetBaseValue(0.0);
        Attributes.Perception.SetBaseValue(0.0);
        Attributes.Intelligence.SetBaseValue(0.0);
        Attributes.Willpower.SetBaseValue(0.0);
        Attributes.Charisma.SetBaseValue(0.0);

        Pronouns = PronounSet.He;
        Locomotion = Locomotion.Amphibious;
        RegenChancePerSegment = 0.002f;
        RegenDice = new Dice(1, 2);
    }

    public override void DepthTraining(int depth, Random random)
    {
        // Player ignoruje depth - dostaje losowe atrybuty z rozkładem dzwonowym
        SetRandomAttribute(Attributes.Strength, random);
        SetRandomAttribute(Attributes.Endurance, random);
        SetRandomAttribute(Attributes.Agility, random);
        SetRandomAttribute(Attributes.Perception, random);
        SetRandomAttribute(Attributes.Intelligence, random);
        SetRandomAttribute(Attributes.Willpower, random);
        SetRandomAttribute(Attributes.Charisma, random);
    }

    private void SetRandomAttribute(Erg.Core.Systems.Attribute attr, Random random)
    {
        // Rozkład dzwonowy 0.1-0.9 (średnia ~0.5)
        double bell = (random.NextDouble() + random.NextDouble() + random.NextDouble()) / 3.0;
        double value = 0.1 + 0.8 * bell;
        attr.SetBaseValue(value);
    }

    public override void OnDeath(Area area)
    {
        // Don't drop inventory - game is ending, we need it for summary
    }
}