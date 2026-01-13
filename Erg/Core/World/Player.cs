namespace Erg.Core.World;

public class Player : Critter
{
    public Player(int x, int y)
        : base("Player", x, y, '@', 0xFFFFFFFF, 0x000000FF, speed: 100)
    {
    }

    public override void OnDeath(Area area)
    {
        // Don't drop inventory - game is ending, we need it for summary
    }
}