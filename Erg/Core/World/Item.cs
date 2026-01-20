using Erg.Core.Abstractions;

namespace Erg.Core.World;

public class Item : Entity
{
    public int Count { get; set; }
    public virtual int Value { get; } = 0;
    public virtual int Mass { get; } = 100;

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

    /// <summary>
    /// Tworzy nowy item tego samego typu z podaną ilością i pozycją.
    /// Używane przy dzieleniu stacków.
    /// </summary>
    public virtual Item SplitStack(int count, int x, int y)
    {
        var type = GetType();

        // Spróbuj konstruktor (x, y, count)
        var ctorWithCount = type.GetConstructor(new[] { typeof(int), typeof(int), typeof(int) });
        if (ctorWithCount != null)
        {
            return (Item)ctorWithCount.Invoke(new object[] { x, y, count });
        }

        // Spróbuj konstruktor (x, y)
        var ctorXY = type.GetConstructor(new[] { typeof(int), typeof(int) });
        if (ctorXY != null)
        {
            var item = (Item)ctorXY.Invoke(new object[] { x, y });
            item.Count = count;
            return item;
        }

        throw new InvalidOperationException($"Cannot split stack of {type.Name}: no suitable constructor found.");
    }
}
