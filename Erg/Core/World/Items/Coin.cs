namespace Erg.Core.World.Items;

public abstract class Coin : Item
{
    public override int Mass => 1;

    protected Coin(string name, int x, int y, uint color, int count = 1)
        : base(name, x, y, '¤', color, 0x000000FF, count)
    {
    }
}
