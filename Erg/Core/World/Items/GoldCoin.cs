namespace Erg.Core.World.Items;

public class GoldCoin : Item
{
    public GoldCoin(int x, int y, int count = 1)
        : base("Gold Coin", x, y, '$', 0xFFD700FF, 0x000000FF, count)
    {
    }
}
