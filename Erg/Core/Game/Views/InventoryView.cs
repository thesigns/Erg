using Erg.Core.Abstractions;
using Erg.Core.Ui;

namespace Erg.Core.Game.Views;

public class InventoryView : IGameView
{
    private Game _game;
    private IGameView _previousView;
    
    public InventoryView(Game game, IGameView previousView)
    {
        _game = game;
        _previousView = previousView;
    }
    
    public void Update(IInput input)
    {
        // ESC lub I wraca do poprzedniego widoku
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Escape) || 
            input.KeyPulse.GetValueOrDefault(KeyboardKey.I))
        {
            _game.SwitchView(_previousView);
        }
    }
    
    public void Render(IOutput output)
    {
        var writer = new Writer(output);

        // Wyczyść ekran
        writer.Clear();

        writer.Locate(2, 2);
        writer.SetForegroundColor(255, 255, 0);
        writer.Write("=== INVENTORY ===");

        var inventory = _game.CurrentSession.Player.Inventory;

        if (inventory.Count == 0)
        {
            writer.Locate(2, 4);
            writer.SetForegroundColor(128, 128, 128);
            writer.Write("(Empty)");
        }
        else
        {
            int maxItems = Math.Min(inventory.Count, 20);
            for (int i = 0; i < maxItems; i++)
            {
                var item = inventory.GetAt(i)!;
                int row = 4 + i;

                // Litera slotu
                writer.Locate(2, row);
                writer.SetForegroundColor(200, 200, 200);
                writer.Write($"{(char)('a' + i)})");

                // Znak itemu w jego kolorze
                writer.Locate(5, row);
                var glyph = item.Glyph;
                byte r = (byte)((glyph.ForegroundColor >> 24) & 0xFF);
                byte g = (byte)((glyph.ForegroundColor >> 16) & 0xFF);
                byte b = (byte)((glyph.ForegroundColor >> 8) & 0xFF);
                writer.SetForegroundColor(r, g, b);
                writer.Write(glyph.Character.ToString());

                // Nazwa i count
                writer.Locate(7, row);
                writer.SetForegroundColor(200, 200, 200);
                if (item.Count > 1)
                    writer.Write($"{item.Name} (x{item.Count})");
                else
                    writer.Write(item.Name);
            }
        }

        writer.Locate(2, output.Rows - 2);
        writer.SetForegroundColor(128, 128, 128);
        writer.Write("Press [ESC] or [I] to close");

        output.SetCursor(0, 0, false);
        output.Render();
    }
}