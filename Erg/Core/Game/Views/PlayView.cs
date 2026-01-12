using Erg.Core.Abstractions;
using Erg.Core.World;

namespace Erg.Core.Game.Views;

public class PlayView : IGameView
{
    private readonly Game _game;
    private Session _session => _game.CurrentSession;
    private bool _awaitingExamineDirection = false;

    public PlayView(Game game)
    {
        _game = game;
    }

    public void Update(IInput input)
    {
        // Blokuj tylko gdy są wiadomości do przewinięcia (nie mieszczą się w 2 liniach)
        if (_session.Messages.NeedsMorePrompt)
        {
            if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Space))
                _session.Messages.ShowNext();
            else if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Enter))
                _session.Messages.SkipAll();
            return; // Blokuj ruch dopóki nie przewinie wszystkich
        }

        // Examine direction selection
        if (_awaitingExamineDirection)
        {
            if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Escape))
            {
                _awaitingExamineDirection = false;
                _session.Messages.Clear();
                return;
            }

            if (TryReadExamineDirection(input, out int edx, out int edy))
            {
                _session.Examine(edx, edy);
                _awaitingExamineDirection = false;
            }
            return; // Block other input while waiting
        }

        // X key - initiate examine
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.X))
        {
            _awaitingExamineDirection = true;
            _session.Messages.Clear();
            _session.Messages.Add("Which direction to examine? (Escape to cancel)");
            return;
        }

        // Obsługa schodów
        bool shiftHeld = input.KeyHeld.GetValueOrDefault(KeyboardKey.LeftShift) ||
                         input.KeyHeld.GetValueOrDefault(KeyboardKey.RightShift);

        // Shift+> - schodzenie w dół
        if (shiftHeld && input.KeyPulse.GetValueOrDefault(KeyboardKey.Period))
        {
            if (_session.IsPlayerOnStairsDown())
            {
                _session.GoDownStairs();
                return;
            }
        }

        // Shift+< - wchodzenie w górę
        if (shiftHeld && input.KeyPulse.GetValueOrDefault(KeyboardKey.Comma))
        {
            if (_session.IsPlayerOnStairsUp())
            {
                if (_session.CurrentLevel == 1)
                {
                    _game.SwitchView(new GameSummaryView(_game));
                    return;
                }
                _session.GoUpStairs();
                return;
            }
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.I))
        {
            _game.SwitchView(new InventoryView(_game, this));
            return;
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.O))
        {
            _session.OpenAdjacentDoors();
            _session.ComputeFov();
            return;
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.C))
        {
            _session.CloseAdjacentDoors();
            _session.ComputeFov();
            return;
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.G))
        {
            _session.PickUpItems();
            return;
        }

        if (TryReadMovement(input, out int dx, out int dy))
        {
            if (_session.TryMovePlayer(dx, dy))
            {
                _session.ComputeFov();
            }
        }
    }

    private bool TryReadMovement(IInput input, out int dx, out int dy)
    {
        dx = dy = 0;

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Left) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp4)) dx = -1;

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Right) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp6)) dx = 1;

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Up) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp8)) dy = -1;

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Down) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp2)) dy = 1;

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp7)) { dx = -1; dy = -1; }
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp9)) { dx = 1; dy = -1; }
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp1)) { dx = -1; dy = 1; }
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp3)) { dx = 1; dy = 1; }

        return dx != 0 || dy != 0;
    }

    private bool TryReadExamineDirection(IInput input, out int dx, out int dy)
    {
        dx = dy = 0;

        // Numpad 5 = examine self (center)
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp5))
            return true;

        // Arrow keys
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Left) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp4)) dx = -1;

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Right) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp6)) dx = 1;

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Up) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp8)) dy = -1;

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Down) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp2)) dy = 1;

        // Diagonals
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp7)) { dx = -1; dy = -1; }
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp9)) { dx = 1; dy = -1; }
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp1)) { dx = -1; dy = 1; }
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp3)) { dx = 1; dy = 1; }

        return dx != 0 || dy != 0;
    }

    public void Render(IOutput output)
    {
        RenderArea(output);
        RenderStatusLine(output);
        RenderMessages(output);
        output.SetCursor(_session.Player.X, _session.Player.Y, true);
        output.Render();
    }

    private void RenderStatusLine(IOutput output)
    {
        // Wyczyść linię 20
        for (int x = 0; x < 80; x++)
        {
            output.PutGlyph(x, 20, new Glyph(' ', 0xFFFFFFFF, 0x000000FF));
        }

        string levelText = $"Level: {_session.Area.Level}";
        RenderTextLine(output, 20, levelText);
    }

    private void RenderMessages(IOutput output)
    {
        var (line1, line2, showMore) = _session.Messages.GetDisplayLines();

        // Wyczyść linie 23-24
        for (int x = 0; x < 80; x++)
        {
            output.PutGlyph(x, 23, new Glyph(' ', 0xFFFFFFFF, 0x000000FF));
            output.PutGlyph(x, 24, new Glyph(' ', 0xFFFFFFFF, 0x000000FF));
        }

        // Renderuj tekst
        RenderTextLine(output, 23, line1);
        RenderTextLine(output, 24, showMore ? line2 + " (more)" : line2);
    }

    private void RenderTextLine(IOutput output, int row, string text)
    {
        for (int i = 0; i < text.Length && i < 80; i++)
        {
            output.PutGlyph(i, row, new Glyph(text[i], 0xFFFFFFFF, 0x000000FF));
        }
    }

    private void RenderArea(IOutput output)
    {
        var area = _session.Area;
        var fov = _session.Fov;

        for (int y = 0; y < area.Height; y++)
        {
            for (int x = 0; x < area.Width; x++)
            {
                bool explored = area.IsExplored(x, y);
                bool seen = fov.IsSeen(x, y);
                var tile = area.GetTile(x, y);

                if (!explored)
                {
                    // Don't show unexplored tiles
                    output.PutGlyph(x, y, new Glyph(' ', 0x000000FF, 0x000000FF));
                }
                else if (!seen)
                {
                    // Show explored but not seen tiles dimmed
                    output.PutGlyph(x, y, DimGlyph(tile.Glyph));
                }
                else
                {
                    // Show seen tiles - tile decides what to display
                    output.PutGlyph(x, y, tile.GetDisplayGlyph());
                }
            }
        }
    }

    private Glyph DimGlyph(Glyph glyph)
    {
        // Dim the foreground color by reducing RGB values
        uint fg = glyph.ForegroundColor;
        byte r = (byte)(((fg >> 24) & 0xFF) / 3);
        byte g = (byte)(((fg >> 16) & 0xFF) / 3);
        byte b = (byte)(((fg >> 8) & 0xFF) / 3);
        byte a = (byte)(fg & 0xFF);
        uint dimmedFg = (uint)((r << 24) | (g << 16) | (b << 8) | a);

        return new Glyph(glyph.Character, dimmedFg, glyph.BackgroundColor);
    }
}