using Erg.Core.Abstractions;
using Erg.Core.World;

namespace Erg.Core.Game.Views;

public class PlayView : IGameView
{
    private readonly Game _game;
    private Session _session => _game.CurrentSession;

    public PlayView(Game game)
    {
        _game = game;
    }

    public void Update(IInput input)
    {
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

    public void Render(IOutput output)
    {
        RenderArea(output);
        RenderEntities(output);
        output.SetCursor(_session.Player.X, _session.Player.Y, true);
        output.Render();
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
                    // Show seen tiles normally
                    output.PutGlyph(x, y, tile.Glyph);
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

    private void RenderEntities(IOutput output)
    {
        var fov = _session.Fov;

        foreach (var entity in _session.Area.Entities)
        {
            // Only show entities in seen tiles
            if (fov.IsSeen(entity.X, entity.Y))
            {
                output.PutGlyph(entity.X, entity.Y, entity.Glyph);
            }
        }
    }
}