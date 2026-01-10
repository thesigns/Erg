using Erg.Core.Abstractions;

namespace Erg.Core.World;

public class Entity
{
    public string Name { get; protected set; }
    public int X { get; protected set; }
    public int Y { get; protected set; }
    public Glyph Glyph { get; protected set; }
    public bool BlocksMovement { get; protected set; }

    protected Entity(
        string name,
        int x,
        int y,
        char character,
        uint foreground,
        uint background,
        bool blocksMovement = true)
    {
        Name = name;
        X = x;
        Y = y;
        Glyph = new Glyph(character, foreground, background);
        BlocksMovement = blocksMovement;
    }

    public virtual void MoveTo(int newX, int newY)
    {
        X = newX;
        Y = newY;
    }

    public virtual void MoveBy(int dx, int dy)
    {
        X += dx;
        Y += dy;
    }
}