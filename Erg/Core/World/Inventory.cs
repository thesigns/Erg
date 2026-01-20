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

    public int RemoveQuantity(Item item, int quantity)
    {
        if (!_items.Contains(item))
            return 0;

        int toRemove = Math.Min(quantity, item.Count);

        if (toRemove >= item.Count)
        {
            _items.Remove(item);
        }
        else
        {
            item.Count -= toRemove;
        }

        return toRemove;
    }

    public Item? GetAt(int index)
    {
        if (index < 0 || index >= _items.Count)
            return null;
        return _items[index];
    }

    public int Count => _items.Count;

    public void Clear()
    {
        _items.Clear();
    }
}
