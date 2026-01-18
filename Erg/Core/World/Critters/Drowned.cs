using Erg.Core.Systems;
using Erg.Core.Types;
using Erg.Core.World.Behaviors;
using Erg.Core.World.Items;

namespace Erg.Core.World.Critters;

public class Drowned : Critter
{
    public override int SightRange => 6;

    public Drowned(int x, int y)
        : base("Drowned", x, y, 'z', 0x4169E1FF, 0x000000FF,
               unarmedDamage: new Dice(1, 6),
               behavior: new TerritorialBehavior(forgetChance: 0.01, territoryRadius: 8))
    {
        Genus = Genus.Zombius;
        Locomotion = Locomotion.Semiaquatic;
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

        // Drowned: wyższe Agility i Strength (pływanie rozwija)
        double highMin = minAmount + 0.25;
        double highMax = maxAmount + 0.25;

        TrainAttrForDepth(Attributes.Strength, Genus.StrengthTraining, highMin, highMax, random);
        TrainAttrForDepth(Attributes.Endurance, Genus.EnduranceTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Agility, Genus.AgilityTraining, highMin, highMax, random);
        TrainAttrForDepth(Attributes.Perception, Genus.PerceptionTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Intelligence, Genus.IntelligenceTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Willpower, Genus.WillpowerTraining, minAmount, maxAmount, random);
        TrainAttrForDepth(Attributes.Charisma, Genus.CharismaTraining, minAmount, maxAmount, random);
    }

    public override void OnDeath(Area area)
    {
        base.OnDeath(area);
        area.AddItem(new DrownedCorpse(X, Y));
    }
}
