using Erg.Core.Systems;
using Erg.Core.World.Behaviors;
using Erg.Core.World.Items;

namespace Erg.Core.World.Critters;

public class Dummy : Critter
{
    public Dummy(int x, int y)
        : base(
            name: "Dummy",
            x: x,
            y: y,
            character: 't',
            fg: 0xD2B48CFF,
            bg: 0x000000FF,
            behavior: PassiveBehavior.Instance)
    {
        Species = Species.Construct;
        Attributes.Agility.SetBaseValue(0.0);  // Speed = 80 * 0.5 = 40
        BaseValue = 12;
        RegenChancePerSegment = 0;
    }

    public override void OnDeath(Area area)
    {
        base.OnDeath(area);
        area.AddItem(new DummyCorpse(X, Y));
    }
}
