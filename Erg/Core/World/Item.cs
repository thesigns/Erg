using Erg.Core.Abstractions;

namespace Erg.Core.World;

public class Item : Entity
{
    public int Count { get; set; }
    public virtual int Value { get; } = 0;

    public Item(
        string name,
        int x,
        int y,
        char character,
        uint foreground,
        uint background,
        int count = 1)
        : base(name, x, y, character, foreground, background, blocksMovement: false)
    {
        Count = count;
    }

    /// <summary>
    /// Określa czy ten item może się stackować z innym.
    /// Podklasy mogą nadpisać dla własnej logiki.
    /// </summary>
    public virtual bool CanStackWith(Item other)
    {
        return GetType() == other.GetType() && Name == other.Name;
    }

    /// <summary>
    /// Dodaje ilość z innego itemu do tego.
    /// </summary>
    public virtual void StackWith(Item other)
    {
        Count += other.Count;
    }
}
