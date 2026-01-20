using Erg.Core.World;
using Erg.Core.World.Items;

namespace Erg.Core.Game.Views.Inventory;

public class InventoryListBuilder
{
    public List<InventoryListEntry> Build(World.Inventory inventory)
    {
        var entries = new List<InventoryListEntry>();

        if (inventory.Count == 0)
            return entries;

        var groups = inventory.Items
            .Select((item, index) => (item, index))
            .GroupBy(x => GetCategory(x.item))
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            entries.Add(new CategoryHeader(group.Key));

            foreach (var (item, index) in group.OrderBy(x => x.item.Name))
            {
                entries.Add(new ItemEntry(item, index));
            }
        }

        return entries;
    }

    private static string GetCategory(Item item) => item switch
    {
        Book => "Books",
        Coin => "Coins",
        Corpse => "Corpses",
        _ => "Miscellaneous"
    };
}
