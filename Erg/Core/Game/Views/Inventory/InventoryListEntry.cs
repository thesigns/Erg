using Erg.Core.World;

namespace Erg.Core.Game.Views.Inventory;

public abstract record InventoryListEntry
{
    public abstract bool IsSelectable { get; }
}

public record CategoryHeader(string CategoryName) : InventoryListEntry
{
    public override bool IsSelectable => false;
}

public record ItemEntry(Item Item, int OriginalIndex) : InventoryListEntry
{
    public override bool IsSelectable => true;
}
