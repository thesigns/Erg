namespace Erg.Core.Abstractions;

public readonly struct Glyph(char character, uint foregroundColor, uint backgroundColor)
{
    public readonly char Character = character;
    public readonly uint ForegroundColor = foregroundColor;
    public readonly uint BackgroundColor = backgroundColor;
}
