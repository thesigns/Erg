using Erg.Core.Abstractions;

namespace Erg.Core.World;

public class Tile
{
    public Glyph Glyph { get; set; }
    public bool Walkable { get; set; }
    public bool Swimmable { get; set; }
    public bool Transparent { get; set; }
    public string Name { get; set; }
    public int RegionId { get; set; }
    public TileType Type { get; set; }
    public TileStructure Structure { get; set; }
    public string? Inscription { get; set; }
    public SpecialEffect SpecialEffect { get; set; } = SpecialEffect.None;

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
        TileType type = TileType.Floor, TileStructure structure = TileStructure.None, bool swimmable = false,
        string? inscription = null)
    {
        Glyph = new Glyph(character, foreground, background);
        Walkable = walkable;
        Swimmable = swimmable;
        Transparent = transparent;
        Name = name;
        Type = type;
        Structure = structure;
        Inscription = inscription;
    }

    public Tile() : this('·', 0x808080FF, 0x000000FF, true, true, "Floor", TileType.Floor) { }

    // Floor factory with structure parameter
    public static Tile Floor(TileStructure structure) =>
        new('·', 0x505050FF, 0x000000FF, true, true, "Floor", TileType.Floor, structure);

    // Dungeon tiles
    public static Tile DungeonWall =>
        new('#', 0x808080FF, 0x000000FF, false, false, "Wall", TileType.Wall);

    public static Tile OpenDoor =>
        new('□', 0x8B4513FF, 0x000000FF, true, true, "Open Door", TileType.OpenDoor, TileStructure.Entrance);

    public static Tile ClosedDoor =>
        new('▣', 0x8B4513FF, 0x000000FF, false, false, "Closed Door", TileType.ClosedDoor, TileStructure.Entrance);

    public static Tile SecretDoor =>
        new('#', 0x808080FF, 0x000000FF, false, false, "Secret Door", TileType.SecretDoor, TileStructure.Entrance);

    // Natural rocks
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

    // Grave tile
    private static readonly string[] GraveInscriptions =
    [
        "This grave is nameless.",
        "R.I.P.",
        "Here lies a forgotten soul.",
        "Unknown adventurer.",
        "Here lies Aldric the Bold. He delved too deep.",
        "Death is but a door. Time is but a window.",
        "The shadows claimed another.",
        "She heard the whispers. Then silence.",
        "Some doors should remain closed.",
        "The gold remains. The owner does not.",
        "Gone to explore the final dungeon.",
        "He died as he lived - running from goblins.",
        "His torch went out at the worst moment.",
        "She forgot to check for traps.",
        "Should have brought more healing potions.",
        "The treasure wasn't worth it.",
        "Do not disturb. Seriously.",
        "What lies beneath hungers still.",
        "He found what he was looking for.",
        "Three went down. None returned.",
        "The dungeon always wins."
    ];

    public static Tile Grave(Random random) =>
        new('†', 0x505050FF, 0x000000FF, true, true, "Grave", TileType.Grave, TileStructure.Room,
            inscription: GraveInscriptions[random.Next(GraveInscriptions.Length)]);
}
