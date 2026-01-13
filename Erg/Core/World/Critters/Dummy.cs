using Erg.Core.World.Behaviors;

namespace Erg.Core.World.Critters;

public class Dummy : Critter
{
    public Dummy(int x, int y)
        : base(
            name: "Dummy",
            x: x,
            y: y,
            character: 'T',
            fg: 0xD2B48CFF,
            bg: 0x000000FF,
            speed: 100,
            behavior: PassiveBehavior.Instance)
    {
    }
}
