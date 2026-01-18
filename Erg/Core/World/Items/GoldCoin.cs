namespace Erg.Core.World.Items;

public class GoldCoin : Coin
{
    public override int Value => 1000;

    public GoldCoin(int x, int y, int count = 1)
        : base("Gold Coin", x, y, 0xFFD700FF, count)
    {
    }
}
