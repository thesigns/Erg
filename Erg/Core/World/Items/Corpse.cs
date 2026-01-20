namespace Erg.Core.World.Items;

public abstract class Corpse : Item
{
    public override int Value => 50;
    public override int Mass => 2000;

    protected Corpse(string name, int x, int y, uint fg, uint bg)
        : base(name, x, y, 'ϗ', fg, bg) { }

    public override bool CanStackWith(Item other) => false;
}
