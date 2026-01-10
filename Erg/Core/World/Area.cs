namespace Erg.Core.World;

public class Area
{
    public int Width { get; }
    public int Height { get; }
    
    private Tile[,] _tiles;
    private bool[,] _explored;
    private List<Entity> _entities;
    
    public Area(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];
        _explored = new bool[width, height];
        _entities = new List<Entity>();
        
        // Inicjalizacja kafli
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                _tiles[x, y] = new Tile(); // Domyślny kafle
            }
        }
    }
    
    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null; // lub Tile.Wall
        return _tiles[x, y];
    }
    
    public void SetTile(int x, int y, Tile tile)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return;
        _tiles[x, y] = tile;
    }

    public bool IsExplored(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        return _explored[x, y];
    }

    public void MarkExplored(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return;
        _explored[x, y] = true;
    }

    public IEnumerable<Entity> Entities => _entities;
    
    public void AddEntity(Entity entity)
    {
        _entities.Add(entity);
    }
    
    public Critter? GetBlockingCritter(int x, int y)
    {
        foreach (var entity in _entities)
        {
            if (entity is Critter critter &&
                critter.BlocksMovement &&
                critter.X == x &&
                critter.Y == y)
            {
                return critter;
            }
        }

        return null;
    }
    
}