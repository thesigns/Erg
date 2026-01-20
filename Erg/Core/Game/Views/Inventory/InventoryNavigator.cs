namespace Erg.Core.Game.Views.Inventory;

public class InventoryNavigator
{
    private readonly List<InventoryListEntry> _entries;
    private int _selectedIndex;
    private int _scrollOffset;

    private const int VisibleLines = 20;

    public int SelectedIndex => _selectedIndex;
    public int ScrollOffset => _scrollOffset;
    public int TotalEntries => _entries.Count;

    public InventoryListEntry? SelectedEntry =>
        _selectedIndex >= 0 && _selectedIndex < _entries.Count
            ? _entries[_selectedIndex]
            : null;

    public InventoryNavigator(List<InventoryListEntry> entries)
    {
        _entries = entries;
        _selectedIndex = FindFirstSelectable(0, 1);
        EnsureVisible();
    }

    public void MoveUp() => Move(-1);
    public void MoveDown() => Move(1);
    public void PageUp() => Move(-VisibleLines);
    public void PageDown() => Move(VisibleLines);

    public void Home()
    {
        int first = FindFirstSelectable(0, 1);
        if (first >= 0)
        {
            _selectedIndex = first;
            EnsureVisible();
        }
    }

    public void End()
    {
        int last = FindFirstSelectable(_entries.Count - 1, -1);
        if (last >= 0)
        {
            _selectedIndex = last;
            EnsureVisible();
        }
    }

    private void Move(int delta)
    {
        if (_entries.Count == 0) return;

        int targetIndex = Math.Clamp(_selectedIndex + delta, 0, _entries.Count - 1);
        int direction = Math.Sign(delta);
        if (direction == 0) direction = 1;

        int newIndex = FindFirstSelectable(targetIndex, direction);

        if (newIndex < 0 && delta > 0)
            newIndex = FindFirstSelectable(_entries.Count - 1, -1);
        else if (newIndex < 0 && delta < 0)
            newIndex = FindFirstSelectable(0, 1);

        if (newIndex >= 0)
        {
            _selectedIndex = newIndex;
            EnsureVisible();
        }
    }

    private int FindFirstSelectable(int start, int direction)
    {
        if (_entries.Count == 0) return -1;

        start = Math.Clamp(start, 0, _entries.Count - 1);

        for (int i = start; i >= 0 && i < _entries.Count; i += direction)
        {
            if (_entries[i].IsSelectable) return i;
        }

        return -1;
    }

    private void EnsureVisible()
    {
        if (_selectedIndex < 0) return;

        if (_selectedIndex < _scrollOffset)
        {
            _scrollOffset = _selectedIndex;
        }
        else if (_selectedIndex >= _scrollOffset + VisibleLines)
        {
            _scrollOffset = _selectedIndex - VisibleLines + 1;
        }

        int maxScroll = Math.Max(0, _entries.Count - VisibleLines);
        _scrollOffset = Math.Min(_scrollOffset, maxScroll);
        _scrollOffset = Math.Max(0, _scrollOffset);
    }

    public IEnumerable<(InventoryListEntry Entry, int Index, bool IsSelected)> GetVisibleEntries()
    {
        for (int i = 0; i < VisibleLines; i++)
        {
            int entryIndex = _scrollOffset + i;
            if (entryIndex >= _entries.Count)
                yield break;

            yield return (_entries[entryIndex], entryIndex, entryIndex == _selectedIndex);
        }
    }
}
