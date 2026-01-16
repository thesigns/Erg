using Erg.Core.Types;

namespace Erg.Core.World;

public class Player : Critter
{
    public override bool CanOpenDoor => true;

    public Player(int x, int y)
        : base("Player", x, y, '@', 0xFFFFFFFF, 0x000000FF, speed: 100, maxHitPoints: 20)
    {
        Pronouns = PronounSet.He;
        Locomotion = Locomotion.Amphibious;
        RegenChancePerSegment = 0.002f;
        RegenDice = new Dice(1, 2);
    }

    public override void OnDeath(Area area)
    {
        // Don't drop inventory - game is ending, we need it for summary
    }
}