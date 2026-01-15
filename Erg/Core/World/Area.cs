using System.Collections.Generic;

namespace Erg.Core.World;

public class Area
{
    public int Width { get; }
    public int Height { get; }
    public int Depth { get; }

    private Tile[,] _tiles;
    private bool[,] _explored;

    public Area(int width, int height, int depth = 1)
    {
        Width = width;
        Height = height;
        Depth = depth;
        _tiles = new Tile[width, height];
        _explored = new bool[width, height];
        
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

    public void SetCritter(Critter critter)
    {
        var tile = GetTile(critter.X, critter.Y);
        if (tile != null)
            tile.Critter = critter;
    }

    public void RemoveCritter(Critter critter)
    {
        var tile = GetTile(critter.X, critter.Y);
        if (tile != null && tile.Critter == critter)
            tile.Critter = null;
    }

    public void MoveCritter(Critter critter, int newX, int newY)
    {
        var oldTile = GetTile(critter.X, critter.Y);
        var newTile = GetTile(newX, newY);

        if (oldTile != null)
            oldTile.Critter = null;

        critter.MoveTo(newX, newY);

        if (newTile != null)
            newTile.Critter = critter;
    }

    public void AddItem(Item item)
    {
        var tile = GetTile(item.X, item.Y);
        if (tile == null) return;

        foreach (var existing in tile.Items)
        {
            if (existing.CanStackWith(item))
            {
                existing.StackWith(item);
                return;
            }
        }

        tile.Items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        var tile = GetTile(item.X, item.Y);
        tile?.Items.Remove(item);
    }

    public Critter? GetCritter(int x, int y)
    {
        return GetTile(x, y)?.Critter;
    }

    public IReadOnlyList<Item> GetItems(int x, int y)
    {
        return (IReadOnlyList<Item>?)GetTile(x, y)?.Items ?? [];
    }

    public Critter? GetBlockingCritter(int x, int y)
    {
        var critter = GetTile(x, y)?.Critter;
        return critter?.BlocksMovement == true ? critter : null;
    }
    
}