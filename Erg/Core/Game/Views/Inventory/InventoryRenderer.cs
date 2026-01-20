using Erg.Core.Ui;
using Erg.Core.World;

namespace Erg.Core.Game.Views.Inventory;

public class InventoryRenderer
{
    private const int HeaderRow = 0;
    private const int ColumnHeaderRow = 1;
    private const int ListStartRow = 2;
    private const int ListEndRow = 21;
    private const int ActionRow = 23;
    private const int NavRow = 24;

    private const int NameCol = 1;
    private const int MassCol = 57;
    private const int ValueCol = 67;
    private const int ScrollCol = 79;

    public void Render(
        Writer writer,
        InventoryNavigator navigator,
        ItemActions availableActions,
        bool quantityInputMode,
        string quantityInput)
    {
        writer.Clear();

        RenderHeader(writer);
        RenderColumnHeaders(writer);
        RenderList(writer, navigator);
        RenderScrollbar(writer, navigator);
        RenderActions(writer, availableActions, quantityInputMode, quantityInput);
        RenderNavigation(writer);
    }

    private void RenderHeader(Writer writer)
    {
        writer.Locate(0, HeaderRow);
        writer.SetForegroundColor(255, 255, 255);
        writer.Write("################################### INVENTORY ##################################");
    }

    private void RenderColumnHeaders(Writer writer)
    {
        writer.Locate(NameCol + 3, ColumnHeaderRow);
        writer.SetForegroundColor(180, 180, 180);
        writer.Write("Item");
        writer.Locate(MassCol, ColumnHeaderRow);
        writer.Write("Mass");
        writer.Locate(ValueCol, ColumnHeaderRow);
        writer.Write("Value");
    }

    private void RenderList(Writer writer, InventoryNavigator navigator)
    {
        int row = ListStartRow;

        foreach (var (entry, index, isSelected) in navigator.GetVisibleEntries())
        {
            if (row > ListEndRow) break;

            if (entry is CategoryHeader header)
            {
                RenderCategoryHeader(writer, row, header);
            }
            else if (entry is ItemEntry itemEntry)
            {
                RenderItemEntry(writer, row, itemEntry, isSelected);
            }

            row++;
        }

        RenderScrollbarColumn(writer, row);
    }

    private void RenderCategoryHeader(Writer writer, int row, CategoryHeader header)
    {
        writer.Locate(NameCol, row);
        writer.SetForegroundColor(100, 180, 180);

        string prefix = $" -- {header.CategoryName} ";
        string line = prefix.PadRight(77, '-') + " ";
        writer.Write(line);

        writer.Locate(ScrollCol, row);
        writer.SetForegroundColor(100, 100, 100);
        writer.Write(":");
    }

    private void RenderItemEntry(Writer writer, int row, ItemEntry entry, bool isSelected)
    {
        var item = entry.Item;

        writer.Locate(NameCol, row);
        if (isSelected)
        {
            writer.SetForegroundColor(255, 255, 0);
            writer.Write(">>");
        }
        else
        {
            writer.SetForegroundColor(100, 100, 100);
            writer.Write("  ");
        }

        writer.Locate(NameCol + 3, row);
        var glyph = item.Glyph;
        byte r = (byte)((glyph.ForegroundColor >> 24) & 0xFF);
        byte g = (byte)((glyph.ForegroundColor >> 16) & 0xFF);
        byte b = (byte)((glyph.ForegroundColor >> 8) & 0xFF);
        writer.SetForegroundColor(r, g, b);
        writer.Write(glyph.Character.ToString());

        writer.Locate(NameCol + 5, row);
        writer.SetForegroundColor(200, 200, 200);
        string name = item.Count > 1 ? $"{item.Name} (x{item.Count})" : item.Name;
        int maxNameLen = MassCol - NameCol - 6;
        if (name.Length > maxNameLen)
            name = name[..maxNameLen];
        writer.Write(name);

        int totalMass = item.Mass * item.Count;
        string massStr = totalMass.ToString();
        writer.Locate(MassCol + 8 - massStr.Length, row);
        writer.SetForegroundColor(150, 150, 150);
        writer.Write(massStr);

        int totalValue = item.Value * item.Count;
        string valueStr = totalValue.ToString();
        writer.Locate(ValueCol + 8 - valueStr.Length, row);
        writer.SetForegroundColor(200, 180, 100);
        writer.Write(valueStr);

        writer.Locate(ScrollCol, row);
        writer.SetForegroundColor(100, 100, 100);
        writer.Write(":");
    }

    private void RenderScrollbarColumn(Writer writer, int startRow)
    {
        for (int row = startRow; row <= ListEndRow; row++)
        {
            writer.Locate(ScrollCol, row);
            writer.SetForegroundColor(100, 100, 100);
            writer.Write(":");
        }
    }

    private void RenderScrollbar(Writer writer, InventoryNavigator navigator)
    {
        int visibleLines = ListEndRow - ListStartRow + 1;
        int total = navigator.TotalEntries;

        if (total <= visibleLines) return;

        float scrollRatio = (float)navigator.ScrollOffset / (total - visibleLines);
        int thumbRow = ListStartRow + (int)(scrollRatio * (visibleLines - 1));

        writer.Locate(ScrollCol, thumbRow);
        writer.SetForegroundColor(200, 200, 200);
        writer.Write("#");
    }

    private void RenderActions(Writer writer, ItemActions actions, bool quantityMode, string input)
    {
        writer.Locate(0, ActionRow);
        writer.SetForegroundColor(200, 200, 200);

        if (quantityMode)
        {
            writer.Write($"Enter quantity: {input}_  [Enter] Confirm  [Esc] Cancel");
        }
        else
        {
            string hint = ItemActionResolver.GetActionHint(actions);
            if (!string.IsNullOrEmpty(hint))
                writer.Write("Actions: " + hint);
        }
    }

    private void RenderNavigation(Writer writer)
    {
        writer.Locate(0, NavRow);
        writer.SetForegroundColor(128, 128, 128);
        writer.Write("[Esc] Quit  [Arrows/Home/End/PgUp/PgDn] Navigate");
    }
}
