using Erg.Core.Abstractions;
using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace Erg.Platforms.Raylib;

public class RaylibOutput : IOutput
{
    public int Cols { get; }
    public int Rows { get; }
    public bool ShouldClose => WindowShouldClose();
    
    private Font Font { get; set; }
    private Vector2 FontSize { get; set; }
    
    private int Width => Cols * (int)FontSize.X;
    private int Height => Rows * (int)FontSize.Y;
    
    private Cell[,] Cells { get; set; }

    private Cursor _cursor = new();

    public RaylibOutput(int cols, int rows, string title, string fontPath, int fontSize)
    {
        Cols = cols;
        Rows = rows;
        Cells = new Cell[cols, rows];
        for (var y = 0; y < Rows; y++)
        for (var x = 0; x < Cols; x++)
            Cells[x, y] = new Cell();
        SetWindowState(ConfigFlags.HiddenWindow);
        InitWindow(1, 1, title);
        SetExitKey(0);
        SetTargetFPS(60);
        SetFont(fontPath, fontSize);
        UpdateWindowSize();
        ClearWindowState(ConfigFlags.HiddenWindow);
    }

    private void SetFont(string fontPath, int fontSize)
    {
        if (IsFontValid(Font)) UnloadFont(Font);

        var cp = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~"; // Ascii
        cp += "ĄĆĘŁŃÓŚŹŻąćęłńóśźż"; // Polish
        cp += "░▒▓│┤╡╢╖╕╣║╗╝╜╛┐└┴┬├─┼╞╟╚╔╩╦╠═╬╧╨╤╥╙╘╒╓╫╪┘┌█▄▌▐▀"; // Box drawing elements
        cp += "·□■▣"; // Other unicode characters
        
        var codepoints = cp.Select(c => (int)c).ToArray();

        Font = LoadFontEx(fontPath, fontSize, codepoints, codepoints.Length);
        FontSize = MeasureTextEx(Font, "W", fontSize, 0);
    }

    private void UpdateWindowSize()
    {
        Console.WriteLine("[Erg] Updating window size: " + Width + "x" + Height);
        SetWindowSize(Width, Height);
        var monitor = GetCurrentMonitor();
        var mw  = GetMonitorWidth(monitor);
        var mh  = GetMonitorHeight(monitor);
        SetWindowPosition( (mw - Width) / 2, (mh - Height) / 2);
    }

    public void PutGlyph(int col, int row, Glyph glyph)
    {
        Cells[col, row].Character = glyph.Character.ToString();
        Cells[col, row].ForegroundColor = UintToColor(glyph.ForegroundColor);
        Cells[col, row].BackgroundColor = UintToColor(glyph.BackgroundColor);
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
        if (_cursor.NextBlinkTime < GetTime())
        {
            _cursor.IsCurrentlyVisible = !_cursor.IsCurrentlyVisible;
            _cursor.NextBlinkTime = GetTime() + _cursor.BlinkInterval;
        }
        
        BeginDrawing();
        ClearBackground(Color.Black);

        for (var y = 0; y < Rows; y++)
        {
            for (var x = 0; x < Cols; x++)
            {
                DrawRectangleV(new Vector2(x * FontSize.X, y * FontSize.Y), new Vector2(FontSize.X, FontSize.Y), Cells[x, y].BackgroundColor);
                DrawTextEx(Font, Cells[x, y].Character, new Vector2(x * FontSize.X, y * FontSize.Y), FontSize.Y, 0, Cells[x, y].ForegroundColor);
            }            
        }

        if (_cursor.Visible && _cursor.IsCurrentlyVisible && _cursor.Size > 0)
        {
            var cursorHeight = (int)(FontSize.Y * _cursor.Size);
            var cursorYOffset = (int)(FontSize.Y * (1 - _cursor.Size));
            BeginBlendMode(BlendMode.SubtractColors);
            DrawRectangle(
                _cursor.Col * (int)FontSize.X,
                _cursor.Row * (int)FontSize.Y + cursorYOffset,
                (int)FontSize.X,
                cursorHeight,
                Cells[_cursor.Col, _cursor.Row].ForegroundColor
                );
            EndBlendMode();
        }
        
        EndDrawing();
    }

    public void SetCursor(int col = 0, int row = 0, bool visible = true)
    {
        if (col < 0 || col >= Cols || row < 0 || row >= Rows) return;
        _cursor.Col = col;
        _cursor.Row = row;
        _cursor.Visible = visible;
    }

    public void Dispose()
    {
        if (IsFontValid(Font))
            UnloadFont(Font);
        CloseWindow();
    }
}