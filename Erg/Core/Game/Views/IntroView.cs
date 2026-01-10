using Erg.Core.Abstractions;
using Erg.Core.Ui;

namespace Erg.Core.Game.Views;

public class IntroView : IGameView
{

    private Game _game;
    
    public IntroView(Game game)
    {
        _game = game;
    }
    
    public void Update(IInput input)
    {
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Space))
        {
            _game.NewSession();
        }
    }

    public void Render(IOutput output)
    {
        var writer = new Writer(output);
        output.SetCursor(0, 0, false);
        writer.Clear();
        writer.Locate(4, 2);
        writer.SetForegroundColor(255, 255, 0);
        writer.Write("Endless roguelike game");
        writer.Locate(4, 3);
        writer.SetForegroundColor(155, 155, 0);
        writer.Write("Development version");
        writer.Locate(4, 5);
        writer.SetForegroundColor(200, 200, 200);
        writer.Write("Press [SPACE] to start");
        output.Render();
    }
}