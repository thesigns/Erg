using Erg.Core.Types;
using Erg.Core.World.Behaviors;

namespace Erg.Core.World.Critters;

public class Zombie : Critter
{
    public override int SightRange => 6;

    public Zombie(int x, int y)
        : base("zombie", x, y, 'z', 0x8B4513FF, 0x000000FF,
               speed: 80, maxHitPoints: 25,
               meleeDamage: new Dice(1, 6),
               behavior: ZombieBehavior.Instance)
    {
        Locomotion = Locomotion.Terrestrial;
    }
}
