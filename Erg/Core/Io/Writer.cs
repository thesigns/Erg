using Erg.Core.Abstractions;

namespace Erg.Core.Ui;

public class Writer
{
    public IOutput Output { get; }
    
    private int X { get; set; } = 0;
    private int Y { get; set; } = 0;
    private uint ForegroundColor { get; set; } = 0xA0A0A0FF;
    private uint BackgroundColor { get; set; } = 0x000000FF;
    public int Cols => Output.Cols;
    public int Rows => Output.Rows;
    
    public Writer(IOutput output) {
        Output = output;    
    }

    public void Clear()
    {
        for (var y = 0; y < Output.Rows; y++)
        {
            for (var x = 0; x < Output.Cols; x++)
            {
                Output.PutGlyph(x, y, new Glyph(' ', 0xFFFFFFFF, 0x000000FF));
            }            
        }
        Locate(0, 0);
    }

    public void Locate(int x, int y)
    {
        if (x < 0 || x >= Cols || y < 0 || y >= Rows) return;
        X = x;
        Y = y;
    }

    public void SetForegroundColor(byte r, byte g, byte b, byte a = 255)
    {
        ForegroundColor = (uint)((r << 24) | (g << 16) | (b << 8) | a);
    }
    
    public void SetBackgroundColor(byte r, byte g, byte b, byte a = 255)
    {
        BackgroundColor = (uint)((r << 24) | (g << 16) | (b << 8) | a);
    }
    
    public void Write(string text) {
        
        foreach (var character in text)
        {
            Output.PutGlyph(X, Y, new Glyph(character, ForegroundColor, BackgroundColor));
            X += 1;
            if (X >= Output.Cols)
            {
                X = 0;
                Y += 1;
            }
            if (Y >= Output.Rows)
            {
                Y = 0;
            }
        }
    }
}