namespace Erg.Core.World.Items;

public abstract class Corpse : Item
{
    protected Corpse(string name, int x, int y, uint fg, uint bg)
        : base(name, x, y, '×', fg, bg) { }

    public override bool CanStackWith(Item other) => false;
}
