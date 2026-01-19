using Erg.Core.Abstractions;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Glyph = Erg.Core.Abstractions.Glyph;

namespace Erg.Platforms.Sfml;

public class SfmlOutput : IOutput, IOverlayRenderer
{
    public int Cols { get; }
    public int Rows { get; }
    public bool ShouldClose => !_window.IsOpen;

    private readonly RenderWindow _window;
    private readonly Font _font;
    private readonly uint _fontSize;
    private readonly Vector2f _cellSize;
    private readonly Vector2f _textOffset;
    private readonly Cell[,] _cells;
    private readonly Clock _clock;
    private readonly Text _textDrawer;
    private readonly RectangleShape _rectDrawer;

    private Cursor _cursor = new();

    private readonly record struct HealthBarOverlay(int Col, int Row, float HealthPercent);
    private readonly List<HealthBarOverlay> _healthBars = new();

    public SfmlOutput(int cols, int rows, string title, string fontPath, uint fontSize)
    {
        Cols = cols;
        Rows = rows;
        _fontSize = fontSize;
        _clock = new Clock();

        _font = new Font(fontPath);
        (_cellSize, _textOffset) = MeasureCharacter();

        var width = (uint)(cols * _cellSize.X);
        var height = (uint)(rows * _cellSize.Y);

        var videoMode = new VideoMode(new Vector2u(width, height));
        _window = new RenderWindow(videoMode, title);
        _window.SetFramerateLimit(60);
        _window.Closed += (_, _) => _window.Close();

        // Center window
        var desktop = VideoMode.DesktopMode;
        var posX = (int)(desktop.Size.X - width) / 2;
        var posY = (int)(desktop.Size.Y - height) / 2;
        _window.Position = new Vector2i(posX, posY);

        _cells = new Cell[cols, rows];
        for (var y = 0; y < Rows; y++)
        for (var x = 0; x < Cols; x++)
            _cells[x, y] = new Cell();

        // Reusable drawing objects
        _textDrawer = new Text(_font, "", _fontSize);
        _rectDrawer = new RectangleShape();
    }

    private (Vector2f size, Vector2f offset) MeasureCharacter()
    {
        using var text = new Text(_font, "#", _fontSize);
        var bounds = text.GetLocalBounds();

        var size = new Vector2f(
            bounds.Width + bounds.Position.X + 2,
            _fontSize * 1.25f  // fontSize + 25% for descenders
        );
        var offset = new Vector2f(bounds.Position.X, bounds.Position.Y);
        return (size, offset);
    }

    public void PutGlyph(int col, int row, Glyph glyph)
    {
        if (col < 0 || col >= Cols || row < 0 || row >= Rows) return;
        _cells[col, row].Character = glyph.Character.ToString();
        _cells[col, row].ForegroundColor = UintToColor(glyph.ForegroundColor);
        _cells[col, row].BackgroundColor = UintToColor(glyph.BackgroundColor);
    }

    private static Color UintToColor(uint rgba)
    {
        var r = (byte)((rgba >> 24) & 0xFF);
        var g = (byte)((rgba >> 16) & 0xFF);
        var b = (byte)((rgba >> 8) & 0xFF);
        var a = (byte)(rgba & 0xFF);
        return new Color(r, g, b, a);
    }

    public void Render()
    {
        _window.DispatchEvents();

        var currentTime = _clock.ElapsedTime.AsSeconds();
        if (_cursor.NextBlinkTime < currentTime)
        {
            _cursor.IsCurrentlyVisible = !_cursor.IsCurrentlyVisible;
            _cursor.NextBlinkTime = currentTime + _cursor.BlinkInterval;
        }

        _window.Clear(Color.Black);

        // Draw cells
        for (var y = 0; y < Rows; y++)
        {
            for (var x = 0; x < Cols; x++)
            {
                var cellX = x * _cellSize.X;
                var cellY = y * _cellSize.Y;

                // Background
                _rectDrawer.Size = _cellSize;
                _rectDrawer.Position = new Vector2f(cellX, cellY);
                _rectDrawer.FillColor = _cells[x, y].BackgroundColor;
                _window.Draw(_rectDrawer);

                // Text
                _textDrawer.DisplayedString = _cells[x, y].Character;
                _textDrawer.Position = new Vector2f(cellX - _textOffset.X, cellY);
                _textDrawer.FillColor = _cells[x, y].ForegroundColor;
                _window.Draw(_textDrawer);
            }
        }

        // Draw health bars
        foreach (var bar in _healthBars)
        {
            DrawHealthBar(bar.Col, bar.Row, bar.HealthPercent);
        }
        _healthBars.Clear();

        // Draw cursor
        if (_cursor.Visible && _cursor.IsCurrentlyVisible && _cursor.Size > 0)
        {
            var cursorHeight = _cellSize.Y * _cursor.Size;
            var cursorYOffset = _cellSize.Y * (1 - _cursor.Size);

            _rectDrawer.Size = new Vector2f(_cellSize.X, cursorHeight);
            _rectDrawer.Position = new Vector2f(
                _cursor.Col * _cellSize.X,
                _cursor.Row * _cellSize.Y + cursorYOffset
            );
            // Invert color effect (simplified - just use white)
            _rectDrawer.FillColor = new Color(255, 255, 255, 128);
            _window.Draw(_rectDrawer);
        }

        _window.Display();
    }

    public void SetCursor(int col = 0, int row = 0, bool visible = true)
    {
        if (col < 0 || col >= Cols || row < 0 || row >= Rows) return;
        _cursor.Col = col;
        _cursor.Row = row;
        _cursor.Visible = visible;
    }

    public void QueueHealthBar(int col, int row, float healthPercent)
    {
        _healthBars.Add(new HealthBarOverlay(col, row, healthPercent));
    }

    public void ClearOverlays()
    {
        _healthBars.Clear();
    }

    private void DrawHealthBar(int col, int row, float healthPercent)
    {
        const int margin = 2;
        const int barHeight = 1;
        const int yOffset = 1;

        float cellX = col * _cellSize.X;
        float cellY = row * _cellSize.Y;
        float barWidth = _cellSize.X - (margin * 2);

        // Red background
        _rectDrawer.Size = new Vector2f(barWidth, barHeight);
        _rectDrawer.Position = new Vector2f(cellX + margin, cellY + yOffset);
        _rectDrawer.FillColor = new Color(180, 0, 0);
        _window.Draw(_rectDrawer);

        // Green HP bar
        float greenWidth = barWidth * Math.Clamp(healthPercent, 0f, 1f);
        if (greenWidth > 0)
        {
            _rectDrawer.Size = new Vector2f(greenWidth, barHeight);
            _rectDrawer.Position = new Vector2f(cellX + margin, cellY + yOffset);
            _rectDrawer.FillColor = new Color(0, 200, 0);
            _window.Draw(_rectDrawer);
        }
    }

    public void Dispose()
    {
        _textDrawer.Dispose();
        _rectDrawer.Dispose();
        _font.Dispose();
        _window.Close();
        _window.Dispose();
    }
}
