using Erg.Core.World.Critters;

namespace Erg.Core.World.Generators;

/// <summary>
/// Populates an Area with critters based on tile types, special effects, and depth.
/// Separated from dungeon shape generation for reusability across different generators.
/// </summary>
public class AreaPopulator
{
    private readonly Area _area;
    private readonly Random _random;

    public AreaPopulator(Area area, Random random)
    {
        _area = area;
        _random = random;
    }

    /// <summary>
    /// Populates the area with critters.
    /// </summary>
    /// <param name="density">Fraction of available positions to populate (0.01 = 1%)</param>
    public void Populate(double density = 0.01)
    {
        var positions = CollectSpawnPositions();
        if (positions.Count == 0) return;

        int count = Math.Max(1, (int)(positions.Count * density));
        for (int i = 0; i < count && positions.Count > 0; i++)
        {
            SpawnOne(positions);
        }
    }

    private List<(int x, int y, bool isWater)> CollectSpawnPositions()
    {
        var positions = new List<(int x, int y, bool isWater)>();

        for (int y = 0; y < _area.Height; y++)
        {
            for (int x = 0; x < _area.Width; x++)
            {
                var tile = _area.GetTile(x, y);
                if (tile == null) continue;

                // Skip non-walkable tiles
                if (!tile.Walkable && !tile.Swimmable) continue;

                // Skip tiles that already have a critter
                if (tile.Critter != null) continue;

                bool isWater = tile.Type == TileType.ShallowWater || tile.Type == TileType.DeepWater;

                positions.Add((x, y, isWater));

                // Add water tiles twice for higher spawn chance
                if (isWater)
                    positions.Add((x, y, true));
            }
        }

        return positions;
    }

    private void SpawnOne(List<(int x, int y, bool isWater)> positions)
    {
        int index = _random.Next(positions.Count);
        var (x, y, isWater) = positions[index];

        // Remove all entries for this position
        positions.RemoveAll(p => p.x == x && p.y == y);

        var tile = _area.GetTile(x, y);
        if (tile?.Critter != null) return;

        var critter = CreateCritterForTile(x, y, tile, isWater);
        critter.DepthTraining(_area.Depth, _random);
        _area.SetCritter(critter);
    }

    private Critter CreateCritterForTile(int x, int y, Tile? tile, bool isWater)
    {
        // Undead aura tiles (graveyards) always spawn zombies
        if (tile?.SpecialEffect == SpecialEffect.UndeadAura)
        {
            return new Zombie(x, y);
        }

        // Water tiles spawn amoebas
        if (isWater)
        {
            return new Amoeba(x, y);
        }

        // Normal tiles: random selection
        int roll = _random.Next(100);
        if (roll < 40)
            return new Dummy(x, y);
        else if (roll < 80)
            return new SpinningDummy(x, y);
        else
            return new Zombie(x, y);
    }
}
