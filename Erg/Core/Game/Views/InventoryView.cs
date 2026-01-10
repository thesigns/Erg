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
        
        writer.Locate(2, 4);
        writer.SetForegroundColor(200, 200, 200);
        writer.Write("(Empty)");
        
        writer.Locate(2, output.Rows - 2);
        writer.SetForegroundColor(128, 128, 128);
        writer.Write("Press [ESC] or [I] to close");
        
        output.SetCursor(0, 0, false);
        output.Render();
    }
}