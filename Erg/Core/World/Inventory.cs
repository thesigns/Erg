namespace Erg.Core.World;

public class Inventory
{
    private readonly List<Item> _items = new();

    public IReadOnlyList<Item> Items => _items;

    public void Add(Item item)
    {
        foreach (var existing in _items)
        {
            if (existing.CanStackWith(item))
            {
                existing.StackWith(item);
                return;
            }
        }

        _items.Add(item);
    }

    public bool Remove(Item item)
    {
        return _items.Remove(item);
    }

    public Item? GetAt(int index)
    {
        if (index < 0 || index >= _items.Count)
            return null;
        return _items[index];
    }

    public int Count => _items.Count;
}
