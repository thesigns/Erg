using Erg.Core.World;
using Erg.Core.World.Items;

namespace Erg.Core.Game.Views.Inventory;

[Flags]
public enum ItemActions
{
    None = 0,
    Drop = 1,
    Read = 2,
    Wield = 4,
    Consume = 8
}

public static class ItemActionResolver
{
    public static ItemActions GetAvailableActions(Item item)
    {
        var actions = ItemActions.Drop | ItemActions.Wield;

        if (item is Book)
            actions |= ItemActions.Read;

        if (item is Corpse)
            actions |= ItemActions.Consume;

        return actions;
    }

    public static string GetActionHint(ItemActions actions)
    {
        if (actions == ItemActions.None)
            return string.Empty;

        var parts = new List<string>();

        if (actions.HasFlag(ItemActions.Drop))
            parts.Add("[dD] Drop");

        if (actions.HasFlag(ItemActions.Read))
            parts.Add("[rR] Read");

        if (actions.HasFlag(ItemActions.Wield))
            parts.Add("[w] Wield");

        if (actions.HasFlag(ItemActions.Consume))
            parts.Add("[cC] Consume");

        return string.Join("  ", parts);
    }
}
