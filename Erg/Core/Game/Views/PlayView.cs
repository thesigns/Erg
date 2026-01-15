using Erg.Core.Abstractions;
using Erg.Core.World;

namespace Erg.Core.Game.Views;

public class PlayView : IGameView
{
    private readonly Game _game;
    private Session _session => _game.CurrentSession;
    private bool _awaitingExamineDirection = false;
    private bool _awaitingAttackConfirmation = false;
    private Critter? _pendingAttackTarget = null;

    // Advanced examine mode
    private bool _advancedExamineMode = false;
    private int _examineX;
    private int _examineY;

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

        // Attack confirmation
        if (_awaitingAttackConfirmation && _pendingAttackTarget != null)
        {
            if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Y))
            {
                _session.PlayerAttack(_pendingAttackTarget);
                _awaitingAttackConfirmation = false;
                _pendingAttackTarget = null;
                if (ProcessTurnAndCheckDeath()) return;
                return;
            }

            if (input.KeyPulse.GetValueOrDefault(KeyboardKey.N) ||
                input.KeyPulse.GetValueOrDefault(KeyboardKey.Escape))
            {
                _awaitingAttackConfirmation = false;
                _pendingAttackTarget = null;
                _session.Messages.Clear();
                return;
            }
            return; // Block other input while waiting
        }

        // Advanced examine mode
        if (_advancedExamineMode)
        {
            if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Escape))
            {
                _advancedExamineMode = false;
                _session.Messages.Clear();
                return;
            }

            if (TryReadDirection(input, out int edx, out int edy))
            {
                int newX = _examineX + edx;
                int newY = _examineY + edy;

                // Constrain to map bounds (80x20)
                if (newX >= 0 && newX < 80 && newY >= 0 && newY < 20)
                {
                    _examineX = newX;
                    _examineY = newY;
                    UpdateExamineDescription();
                }
            }
            return; // Block other input while in advanced examine mode
        }

        // Alt+X - advanced examine mode
        bool altHeld = input.KeyHeld.GetValueOrDefault(KeyboardKey.LeftAlt) ||
                       input.KeyHeld.GetValueOrDefault(KeyboardKey.RightAlt);
        if (altHeld && input.KeyPulse.GetValueOrDefault(KeyboardKey.X))
        {
            _advancedExamineMode = true;
            _examineX = _session.Player.X;
            _examineY = _session.Player.Y;
            UpdateExamineDescription();
            return;
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
                if (ProcessTurnAndCheckDeath()) return;
                return;
            }
        }

        // Shift+< - wchodzenie w górę
        if (shiftHeld && input.KeyPulse.GetValueOrDefault(KeyboardKey.Comma))
        {
            if (_session.IsPlayerOnStairsUp())
            {
                if (_session.CurrentDepth == 1)
                {
                    _game.SwitchView(new GameSummaryView(_game, GameEndReason.Escaped));
                    return;
                }
                _session.GoUpStairs();
                if (ProcessTurnAndCheckDeath()) return;
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
            if (ProcessTurnAndCheckDeath()) return;
            return;
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.C))
        {
            _session.CloseAdjacentDoors();
            _session.ComputeFov();
            if (ProcessTurnAndCheckDeath()) return;
            return;
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.G))
        {
            _session.PickUpItems();
            if (ProcessTurnAndCheckDeath()) return;
            return;
        }

        // Numpad 5 - Wait
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp5))
        {
            _session.PlayerWait();
            if (ProcessTurnAndCheckDeath()) return;
            return;
        }

        if (TryReadMovement(input, out int dx, out int dy))
        {
            var result = _session.TryMoveOrAttack(dx, dy);

            if (result.Moved)
            {
                _session.ComputeFov();
                if (ProcessTurnAndCheckDeath(result.EnergyCost)) return;
            }
            else if (result.NeedsConfirmation && result.Target != null)
            {
                _awaitingAttackConfirmation = true;
                _pendingAttackTarget = result.Target;
            }
            else if (result.Attacked)
            {
                if (ProcessTurnAndCheckDeath()) return;
            }
        }
    }

    private bool ProcessTurnAndCheckDeath(int energyCost = 1000)
    {
        _session.IncrementTurn();
        _session.ProcessCritterTurns(energyCost);
        if (!_session.Player.IsAlive)
        {
            _game.SwitchView(new GameSummaryView(_game, GameEndReason.Died));
            return true;
        }
        return false;
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

    private bool TryReadDirection(IInput input, out int dx, out int dy)
    {
        dx = dy = 0;

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

    private void UpdateExamineDescription()
    {
        _session.Messages.Clear();

        if (!_session.Fov.IsSeen(_examineX, _examineY))
        {
            _session.Messages.Add("You don't see this area.");
            return;
        }

        _session.ExamineAt(_examineX, _examineY);
    }

    public void Render(IOutput output)
    {
        RenderArea(output);
        RenderStatusLine(output);
        RenderMessages(output);

        if (_advancedExamineMode)
            output.SetCursor(_examineX, _examineY + 2, true);
        else
            output.SetCursor(_session.Player.X, _session.Player.Y + 2, true);

        output.Render();
    }

    private void RenderStatusLine(IOutput output)
    {
        // Wyczyść linie 22-24
        for (int x = 0; x < 80; x++)
        {
            output.PutGlyph(x, 22, new Glyph(' ', 0xFFFFFFFF, 0x000000FF));
            output.PutGlyph(x, 23, new Glyph(' ', 0xFFFFFFFF, 0x000000FF));
            output.PutGlyph(x, 24, new Glyph(' ', 0xFFFFFFFF, 0x000000FF));
        }

        string depthText = $"Depth: {_session.Area.Depth}";
        RenderTextLine(output, 22, depthText);

        var player = _session.Player;
        string statsText = $"XPL: {player.ExperienceLevel}  HP: {player.HitPoints}/{player.MaxHitPoints}";
        RenderTextLine(output, 23, statsText);

        string speedText = $"Sp: {player.Speed}";
        RenderTextLine(output, 24, speedText);
    }

    private void RenderMessages(IOutput output)
    {
        var (line1, line2, showMore) = _session.Messages.GetDisplayLines();

        // Wyczyść linie 0-1
        for (int x = 0; x < 80; x++)
        {
            output.PutGlyph(x, 0, new Glyph(' ', 0xFFFFFFFF, 0x000000FF));
            output.PutGlyph(x, 1, new Glyph(' ', 0xFFFFFFFF, 0x000000FF));
        }

        // Renderuj tekst
        RenderTextLine(output, 0, line1);
        RenderTextLine(output, 1, showMore ? line2 + " (more)" : line2);
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
                    output.PutGlyph(x, y + 2, new Glyph(' ', 0x505050FF, 0x000000FF));
                }
                else if (!seen)
                {
                    // Show explored but not seen tiles dimmed
                    output.PutGlyph(x, y + 2, DimGlyph(tile.Glyph));
                }
                else
                {
                    // Show seen tiles - tile decides what to display
                    output.PutGlyph(x, y + 2, tile.GetDisplayGlyph());
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