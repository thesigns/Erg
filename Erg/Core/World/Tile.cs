using Erg.Core.Abstractions;

namespace Erg.Core.World;

public class Tile
{
    public Glyph Glyph { get; set; }
    public bool Walkable { get; set; }
    public bool Swimable { get; set; }
    public bool Transparent { get; set; }
    public string Name { get; set; }
    public int RegionId { get; set; }
    public TileType Type { get; set; }
    public TileStructure Structure { get; set; }

    public Critter? Critter { get; set; }
    public List<Item> Items { get; } = new();

    public Glyph GetDisplayGlyph()
    {
        if (Critter != null)
            return Critter.Glyph;

        if (Items.Count >= 2)
            return new Glyph('%', 0x8B4513FF, 0x000000FF);

        if (Items.Count == 1)
            return Items[0].Glyph;

        return Glyph;
    }

    public Tile(char character, uint foreground, uint background, bool walkable, bool transparent, string name,
        TileType type = TileType.Floor, TileStructure structure = TileStructure.None, bool swimable = false)
    {
        Glyph = new Glyph(character, foreground, background);
        Walkable = walkable;
        Swimable = swimable;
        Transparent = transparent;
        Name = name;
        Type = type;
        Structure = structure;
    }

    public Tile() : this('.', 0x808080FF, 0x000000FF, true, true, "Floor", TileType.Floor) { }

    // Floor factory with structure parameter
    public static Tile Floor(TileStructure structure) =>
        new('.', 0x505050FF, 0x000000FF, true, true, "Floor", TileType.Floor, structure);

    // Dungeon tiles
    public static Tile DungeonWall =>
        new('#', 0x808080FF, 0x000000FF, false, false, "Wall", TileType.Wall);

    public static Tile OpenDoor =>
        new('□', 0x8B4513FF, 0x000000FF, true, true, "Open Door", TileType.OpenDoor, TileStructure.Entrance);

    public static Tile ClosedDoor =>
        new('▣', 0x8B4513FF, 0x000000FF, false, false, "Closed Door", TileType.ClosedDoor, TileStructure.Entrance);

    public static Tile SecretDoor =>
        new('#', 0x808080FF, 0x000000FF, false, false, "Secret Door", TileType.SecretDoor, TileStructure.Entrance);

    // For future use (Etap 2)
    public static Tile Rock =>
        new('#', 0x505050FF, 0x000000FF, false, false, "Rock", TileType.Rock);

    public static Tile ImpenetrableRock =>
        new('#', 0x505050FF, 0x000000FF, false, false, "Impenetrable Rock", TileType.ImpenetrableRock);

    // Stairs tiles
    public static Tile StairsUp =>
        new('<', 0xC0C0C0FF, 0x000000FF, true, true, "Stairs Up", TileType.StairsUp, TileStructure.Room);

    public static Tile StairsDown =>
        new('>', 0xC0C0C0FF, 0x000000FF, true, true, "Stairs Down", TileType.StairsDown, TileStructure.Room);

    // Water tiles
    public static Tile ShallowWater =>
        new('≈', 0x2848FFFF, 0x000000FF, true, true, "Shallow Water", TileType.ShallowWater);

    public static Tile DeepWater =>
        new('≈', 0x1020AAFF, 0x000000FF, false, true, "Deep Water", TileType.DeepWater, TileStructure.None, true);
}
