using Erg.Core.Abstractions;
using Erg.Core.Ui;
using Erg.Core.World.Items;

namespace Erg.Core.Game.Views;

public class GameSummaryView : IGameView
{
    private readonly Game _game;
    private readonly GameEndReason _reason;
    private readonly Session _session;

    public GameSummaryView(Game game, GameEndReason reason)
    {
        _game = game;
        _reason = reason;
        _session = game.CurrentSession;
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

        int row = 3;

        // Header
        if (_reason == GameEndReason.Died)
        {
            writer.Locate(30, row);
            writer.SetForegroundColor(200, 50, 50);
            writer.Write("+ REST IN PEACE +");
        }
        else
        {
            writer.Locate(33, row);
            writer.SetForegroundColor(255, 215, 0);
            writer.Write("* ESCAPED *");
        }

        row += 3;

        // Stats
        writer.SetForegroundColor(180, 180, 180);

        writer.Locate(20, row);
        writer.Write($"You explored down to dungeon level {_session.DeepestLevel}.");
        row += 2;

        writer.Locate(20, row);
        writer.Write($"You survived for {_session.TurnCount} turns.");
        row += 3;

        // Cause of end
        writer.SetForegroundColor(220, 220, 220);
        writer.Locate(20, row);

        if (_reason == GameEndReason.Died)
        {
            var killer = _session.Player.KilledBy;
            if (killer != null)
                writer.Write($"You were slain by a {killer.Name}.");
            else
                writer.Write("You died of mysterious causes.");
        }
        else
        {
            writer.Write("You decided to leave the dungeon.");
        }

        row += 3;

        // Gold
        int goldCount = CountGold();
        writer.SetForegroundColor(255, 215, 0);
        writer.Locate(20, row);
        if (goldCount > 0)
            writer.Write($"Gold collected: {goldCount} coins");
        else
            writer.Write("Gold collected: none");

        row += 4;

        // Footer
        writer.SetForegroundColor(100, 100, 100);
        writer.Locate(24, row);
        writer.Write("Press [Space] to return to title");

        output.Render();
    }

    private int CountGold()
    {
        int total = 0;
        foreach (var item in _session.Player.Inventory.Items)
        {
            if (item is GoldCoin gold)
                total += gold.Count;
        }
        return total;
    }
}
