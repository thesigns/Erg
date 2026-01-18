namespace Erg.Core.World.Items;

public class SilverCoin : Coin
{
    public override int Value => 50;

    public SilverCoin(int x, int y, int count = 1)
        : base("Silver Coin", x, y, 0xC0C0C0FF, count)
    {
    }
}
