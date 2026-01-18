using Erg.Core.Systems;
using Erg.Core.Types;
using Erg.Core.World.Behaviors;

namespace Erg.Core.World.Critters;

public class Hanged : Critter
{
    public override int SightRange => 8;

    public Hanged(int x, int y)
        : base("hanged", x, y, 'z', 0x8B008BFF, 0x000000FF,
               unarmedDamage: new Dice(1, 8),
               behavior: new TerritorialBehavior(forgetChance: 0.002, territoryRadius: 4))
    {
        Genus = Genus.Zombius;
        Locomotion = Locomotion.Terrestrial;
    }
}
