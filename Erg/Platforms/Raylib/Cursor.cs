namespace Erg.Platforms.Raylib;

public struct Cursor()
{
    public int Col;
    public int Row;
    public bool Visible = true;
    public float Size = 0.15f;
    public bool IsCurrentlyVisible;
    public double BlinkInterval = 0.4;
    public double NextBlinkTime;
}
