namespace Erg.Core.Abstractions;

public interface IOutput : IDisposable
{
    int Cols { get; }
    int Rows { get; }
    bool ShouldClose { get; }
    void PutGlyph(int col, int row, Glyph glyph);
    void SetCursor(int col, int row, bool visible = true);
    void Render();
}