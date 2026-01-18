using Erg.Core.Game;

namespace Erg.Core.World.Items;

/// <summary>
/// Base class for readable books. Books train skills when read.
/// </summary>
public abstract class Book : Item
{
    public override int Value => 200;

    protected Book(string name, int x, int y)
        : base(name, x, y, '«', 0x8B4513FF, 0x000000FF) { }

    public override bool CanStackWith(Item other) => other.GetType() == GetType();

    /// <summary>
    /// Called when a critter reads this book.
    /// Returns true if the book should be consumed after reading.
    /// </summary>
    public abstract bool OnRead(Critter reader, Session session);
}
