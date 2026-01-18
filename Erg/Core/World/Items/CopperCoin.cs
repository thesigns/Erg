namespace Erg.Core.World.Items;

public class CopperCoin : Coin
{
    public override int Value => 5;

    public CopperCoin(int x, int y, int count = 1)
        : base("Copper Coin", x, y, 0xB87333FF, count)
    {
    }
}
