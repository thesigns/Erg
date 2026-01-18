using Erg.Core.Systems;
using Erg.Core.Types;
using Erg.Core.World.Behaviors;

namespace Erg.Core.World.Critters;

public class Drowned : Critter
{
    public override int SightRange => 6;

    public Drowned(int x, int y)
        : base("drowned", x, y, 'z', 0x4169E1FF, 0x000000FF,
               unarmedDamage: new Dice(1, 6),
               behavior: new TerritorialBehavior(forgetChance: 0.01, territoryRadius: 8))
    {
        Genus = Genus.Zombius;
        Locomotion = Locomotion.Semiaquatic;
    }
}
