using Erg.Core.Systems;
using Erg.Core.Types;
using Erg.Core.World.Behaviors;
using Erg.Core.World.Items;

namespace Erg.Core.World.Critters;

public class Zombie : Critter
{
    public override int SightRange => 6;

    public Zombie(int x, int y)
        : base("zombie", x, y, 'z', 0x8B4513FF, 0x000000FF,
               unarmedDamage: new Dice(1, 6),
               behavior: ZombieBehavior.Instance)
    {
        Genus = Genus.Risen;
        Locomotion = Locomotion.Terrestrial;
    }

    public override void OnDeath(Area area)
    {
        base.OnDeath(area);
        area.AddItem(new ZombieCorpse(X, Y));
    }
}
