using Erg.Core.Abstractions;

namespace Erg.Core.World;

public class Tile
{
    public Glyph Glyph { get; set; }
    public bool Walkable { get; set; }
    public bool Transparent { get; set; }
    public string Name { get; set; }
    
    public Tile(char character, uint foreground, uint background, bool walkable, bool transparent, string name)
    {
        Glyph = new Glyph(character, foreground, background);
        Walkable = walkable;
        Transparent = transparent;
        Name = name;
    }
    
    public Tile() : this('.', 0x808080FF, 0x000000FF, true, true, "Floor") { }
    
    // Statyczne predefiniowane kafle
    public static Tile Floor => new('.', 0x808080FF, 0x000000FF, true, true, "Floor");
    public static Tile Wall => new('#', 0xFFFFFFFF, 0x000000FF, false, false, "Wall");
    public static Tile Door => new('+', 0x8B4513FF, 0x000000FF, true, false, "Door");

    // Dungeon tiles
    public static Tile DungeonFloor => new('.', 0x505050FF, 0x000000FF, true, true, "Floor");
    public static Tile DungeonWall => new('#', 0x808080FF, 0x000000FF, false, false, "Wall");
    public static Tile OpenDoor => new('/', 0x8B4513FF, 0x000000FF, true, true, "Open Door");
    public static Tile ClosedDoor => new('+', 0x8B4513FF, 0x000000FF, false, false, "Closed Door");
}
