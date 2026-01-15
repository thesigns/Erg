using Erg.Core.Types;
using Erg.Core.World.Behaviors;
using Erg.Core.World.Items;

namespace Erg.Core.World.Critters;

public class Amoeba : Critter
{
    public Amoeba(int x, int y)
        : base("Amoeba", x, y, 'j', 0x20B2AAFF, 0x000000FF,
               speed: 90, maxHitPoints: 10, behavior: AmoebaBehavior.Instance)
    {
        Locomotion = Locomotion.Semiaquatic;
        BaseValue = 40;
        RegenChancePerSegment = 0.02f;
        RegenDice = new Dice(1, 3);
    }

    public override void OnDeath(Area area)
    {
        base.OnDeath(area);
        area.AddItem(new AmoebaCorpse(X, Y));
    }
}
