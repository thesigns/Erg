using Erg.Core.Abstractions;
using Erg.Core.Ui;

namespace Erg.Core.Game.Views;

public class GameSummaryView : IGameView
{
    private readonly Game _game;

    public GameSummaryView(Game game)
    {
        _game = game;
    }

    public void Update(IInput input)
    {
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Space))
        {
            _game.SwitchView(new IntroView(_game));
        }
    }

    public void Render(IOutput output)
    {
        var writer = new Writer(output);
        output.SetCursor(0, 0, false);
        writer.Clear();

        writer.Locate(4, 2);
        writer.SetForegroundColor(255, 255, 0);
        writer.Write("Game Summary");

        writer.Locate(4, 5);
        writer.SetForegroundColor(200, 200, 200);
        writer.Write("Press [Space] to quit");

        output.Render();
    }
}
