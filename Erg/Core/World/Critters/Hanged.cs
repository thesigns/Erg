using Erg.Core.Systems;
using Erg.Core.Types;
using Erg.Core.World.Behaviors;

namespace Erg.Core.World.Critters;

public class Hanged : Critter
{
    public override int SightRange => 8;

    public Hanged(int x, int y)
        : base("Hanged", x, y, 'z', 0x8B008BFF, 0x000000FF,
               unarmedDamage: new Dice(1, 8),
               behavior: new TerritorialBehavior(forgetChance: 0.002, territoryRadius: 4))
    {
        Genus = Genus.Zombius;
        Locomotion = Locomotion.Terrestrial;
    }

    public override void DepthTraining(int depth, Random random)
    {
        int depthDelta = Math.Max(0, depth - MinDepth);

        double baseMin = 0.2;
        double baseMax = 0.5;
        double depthMin = depthDelta * 0.02;
        double depthMax = depthDelta * 0.05;

        double minAmount = baseMin + depthMin;
        double maxAmount = baseMax + depthMax;

        // Hanged: wiszenie daje czas do namysłu
        double highMin = minAmount + 0.25;
        double highMax = maxAmount + 0.25;
        double veryHighMin = minAmount + 0.35;
        double veryHighMax = maxAmount + 0.35;
        double lowMin = minAmount - 0.1;
        double lowMax = maxAmount - 0.1;

        TrainAttrForDepth(Attributes.Strength, Genus.StrengthTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Endurance, Genus.EnduranceTraining, veryHighMin, veryHighMax, random);
        TrainAttrForDepth(Attributes.Agility, Genus.AgilityTraining, lowMin, lowMax, random);
        TrainAttrForDepth(Attributes.Perception, Genus.PerceptionTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Intelligence, Genus.IntelligenceTraining, highMin, highMax, random);
        TrainAttrForDepth(Attributes.Willpower, Genus.WillpowerTraining, highMin, highMax, random);
        TrainAttrForDepth(Attributes.Charisma, Genus.CharismaTraining, minAmount, maxAmount, random);
    }
}
