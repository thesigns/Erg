using Erg.Core.World.Behaviors;

namespace Erg.Core.World.Critters;

public class SpinningDummy : Critter
{
    public SpinningDummy(int x, int y)
        : base(
            name: "Spinning Dummy",
            x: x,
            y: y,
            character: 'T',
            fg: 0xFF4444FF,  // Czerwony (jasny)
            bg: 0x000000FF,
            speed: 200,
            behavior: new SpinAttackBehavior())
    {
    }
}
