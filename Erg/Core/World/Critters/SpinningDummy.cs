using Erg.Core.Systems;
using Erg.Core.World.Behaviors;
using Erg.Core.World.Items;

namespace Erg.Core.World.Critters;

public class SpinningDummy : Critter
{
    public SpinningDummy(int x, int y)
        : base(
            name: "Spinning Dummy",
            x: x,
            y: y,
            character: 't',
            fg: 0xFF4444FF,  // Czerwony (jasny)
            bg: 0x000000FF,
            behavior: new SpinAttackBehavior())
    {
        Species = Species.Construct;
        Attributes.Agility.SetBaseValue(0.9);  // Speed = 80 * 1.4 = 112
        BaseValue = 20;
        RegenChancePerSegment = 0;
    }

    public override void OnDeath(Area area)
    {
        base.OnDeath(area);
        area.AddItem(new SpinningDummyCorpse(X, Y));
    }
}
