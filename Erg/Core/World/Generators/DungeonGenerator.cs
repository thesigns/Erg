using System;
using System.Collections.Generic;
using System.Linq;
using Erg.Core.World.Items;

namespace Erg.Core.World.Generators;

public class DungeonGenerator : IDungeonGenerator
{
    private readonly int _width;
    private readonly int _height;
    private readonly Random _random;

    private HashSet<(int x, int y)> _floorTiles = new();
    private (int x, int y) _playerStart;
    private int _nextRegionId = 1;

    public DungeonGenerator(int width, int height, Random random)
    {
        _width = width;
        _height = height;
        _random = random;
    }

    public Area Generate()
    {
        var area = new Area(_width, _height);

        // Fill with walls
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                area.SetTile(x, y, Tile.DungeonWall);
            }
        }

        // Generate first room
        int roomWidth = _random.Next(6, 12);
        int roomHeight = _random.Next(4, 8);
        int roomX = _random.Next(1, _width - roomWidth - 1);
        int roomY = _random.Next(1, _height - roomHeight - 1);
        CarveRoom(area, roomX, roomY, roomWidth, roomHeight, _nextRegionId++);

        _playerStart = (roomX + roomWidth / 2, roomY + roomHeight / 2);

        // Try to add more rooms with corridors
        int failures = 0;
        while (failures < 200)
        {
            if (TryAddCorridorAndRoom(area))
            {
                failures = 0;
            }
            else
            {
                failures++;
            }
        }

        PlaceItems(area);

        return area;
    }

    public (int x, int y) GetPlayerStartPosition()
    {
        return _playerStart;
    }

    private void CarveRoom(Area area, int roomX, int roomY, int width, int height, int regionId)
    {
        for (int y = roomY; y < roomY + height; y++)
        {
            for (int x = roomX; x < roomX + width; x++)
            {
                var tile = Tile.DungeonFloor;
                tile.RegionId = regionId;
                area.SetTile(x, y, tile);
                _floorTiles.Add((x, y));
            }
        }
    }

    private bool TryAddCorridorAndRoom(Area area)
    {
        // Find all valid room walls (not corners)
        var roomWalls = GetRoomWalls();
        if (roomWalls.Count == 0)
            return false;

        // Pick a random wall
        var (wallX, wallY, dirX, dirY) = roomWalls[_random.Next(roomWalls.Count)];

        // Pre-plan corridor
        int corridorLength = _random.Next(3, 16);
        var corridorTiles = new List<(int x, int y, bool isEntrance)>();

        // Start with the wall tile itself (entrance from source room)
        corridorTiles.Add((wallX, wallY, true));

        int x = wallX;
        int y = wallY;
        int currentDirX = dirX;
        int currentDirY = dirY;

        // Decide if we turn and when
        bool willTurn = _random.Next(100) < 20;
        int turnAfter = willTurn ? _random.Next(5, 11) : corridorLength + 1;

        for (int i = 0; i < corridorLength; i++)
        {
            x += currentDirX;
            y += currentDirY;

            // Check bounds (leave 1 tile margin for room walls)
            if (x <= 1 || x >= _width - 2 || y <= 1 || y >= _height - 2)
                return false;

            // Check if overlaps existing floor
            if (_floorTiles.Contains((x, y)))
                return false;

            // Check perpendicular tiles (corridor sides) don't touch existing floor
            if (currentDirX != 0) // Moving horizontally - check above/below
            {
                if (_floorTiles.Contains((x, y - 1)) || _floorTiles.Contains((x, y + 1)))
                    return false;
            }
            else // Moving vertically - check left/right
            {
                if (_floorTiles.Contains((x - 1, y)) || _floorTiles.Contains((x + 1, y)))
                    return false;
            }

            corridorTiles.Add((x, y, false));

            // Turn if needed
            if (i == turnAfter - 1 && willTurn)
            {
                // Turn perpendicular
                if (currentDirX != 0)
                {
                    currentDirX = 0;
                    currentDirY = _random.Next(2) == 0 ? -1 : 1;
                }
                else
                {
                    currentDirY = 0;
                    currentDirX = _random.Next(2) == 0 ? -1 : 1;
                }
            }
        }

        // Add entrance tile to the new room
        int entranceX = x + currentDirX;
        int entranceY = y + currentDirY;

        if (_floorTiles.Contains((entranceX, entranceY)))
            return false;

        // Check perpendicular tiles for entrance
        if (currentDirX != 0) // Moving horizontally
        {
            if (_floorTiles.Contains((entranceX, entranceY - 1)) || _floorTiles.Contains((entranceX, entranceY + 1)))
                return false;
        }
        else // Moving vertically
        {
            if (_floorTiles.Contains((entranceX - 1, entranceY)) || _floorTiles.Contains((entranceX + 1, entranceY)))
                return false;
        }

        corridorTiles.Add((entranceX, entranceY, true));

        // Try to fit a room at the end
        int newRoomWidth = _random.Next(4, 10);
        int newRoomHeight = _random.Next(3, 6);

        // Position room based on corridor end direction (after entrance tile)
        int roomX, roomY;
        if (currentDirX == 1) // Going right
        {
            roomX = entranceX + 1;
            roomY = entranceY - newRoomHeight / 2;
        }
        else if (currentDirX == -1) // Going left
        {
            roomX = entranceX - newRoomWidth;
            roomY = entranceY - newRoomHeight / 2;
        }
        else if (currentDirY == 1) // Going down
        {
            roomX = entranceX - newRoomWidth / 2;
            roomY = entranceY + 1;
        }
        else // Going up
        {
            roomX = entranceX - newRoomWidth / 2;
            roomY = entranceY - newRoomHeight;
        }

        // Check if room fits
        if (!RoomFits(roomX, roomY, newRoomWidth, newRoomHeight))
            return false;

        // Everything fits - carve corridor and room
        // Assign region IDs: each entrance gets its own, corridor tiles share one
        int corridorRegionId = _nextRegionId++;

        for (int i = 0; i < corridorTiles.Count; i++)
        {
            var (cx, cy, isEntrance) = corridorTiles[i];
            Tile tile;

            if (isEntrance)
            {
                tile = GetRandomEntrance();
                tile.RegionId = _nextRegionId++; // Each entrance gets unique region
            }
            else
            {
                tile = Tile.DungeonFloor;
                tile.RegionId = corridorRegionId; // Corridor tiles share region
            }

            area.SetTile(cx, cy, tile);
            _floorTiles.Add((cx, cy));
        }

        CarveRoom(area, roomX, roomY, newRoomWidth, newRoomHeight, _nextRegionId++);

        return true;
    }

    private Tile GetRandomEntrance()
    {
        return _random.Next(3) switch
        {
            0 => Tile.DungeonFloor,
            1 => Tile.OpenDoor,
            _ => Tile.ClosedDoor
        };
    }

    private List<(int x, int y, int dirX, int dirY)> GetRoomWalls()
    {
        var walls = new List<(int x, int y, int dirX, int dirY)>();

        foreach (var (fx, fy) in _floorTiles)
        {
            // Check each cardinal direction
            CheckWall(walls, fx, fy, -1, 0); // Left
            CheckWall(walls, fx, fy, 1, 0);  // Right
            CheckWall(walls, fx, fy, 0, -1); // Up
            CheckWall(walls, fx, fy, 0, 1);  // Down
        }

        return walls;
    }

    private void CheckWall(List<(int x, int y, int dirX, int dirY)> walls, int fx, int fy, int dirX, int dirY)
    {
        int wx = fx + dirX;
        int wy = fy + dirY;

        // Must be a wall (not floor)
        if (_floorTiles.Contains((wx, wy)))
            return;

        // Must be in bounds
        if (wx <= 0 || wx >= _width - 1 || wy <= 0 || wy >= _height - 1)
            return;

        // Not a corner - check that perpendicular neighbors of the floor tile are also floor
        // This ensures we're on a flat wall, not a corner
        if (dirX != 0) // Horizontal direction - check vertical neighbors
        {
            if (!_floorTiles.Contains((fx, fy - 1)) || !_floorTiles.Contains((fx, fy + 1)))
                return;
        }
        else // Vertical direction - check horizontal neighbors
        {
            if (!_floorTiles.Contains((fx - 1, fy)) || !_floorTiles.Contains((fx + 1, fy)))
                return;
        }

        walls.Add((wx, wy, dirX, dirY));
    }

    private bool RoomFits(int roomX, int roomY, int width, int height)
    {
        // Check bounds (with 1 tile margin for walls)
        if (roomX <= 1 || roomY <= 1 ||
            roomX + width >= _width - 2 || roomY + height >= _height - 2)
            return false;

        // Check no overlap with existing floor, including 1-tile buffer for walls
        for (int y = roomY - 1; y < roomY + height + 1; y++)
        {
            for (int x = roomX - 1; x < roomX + width + 1; x++)
            {
                if (_floorTiles.Contains((x, y)))
                    return false;
            }
        }

        return true;
    }

    private void PlaceItems(Area area)
    {
        var availableFloors = _floorTiles
            .Where(f => f != _playerStart)
            .ToList();

        int coinCount = _random.Next(5, 15);

        for (int i = 0; i < coinCount && availableFloors.Count > 0; i++)
        {
            int index = _random.Next(availableFloors.Count);
            var (x, y) = availableFloors[index];
            availableFloors.RemoveAt(index);

            int amount = _random.Next(1, 101);
            var coin = new GoldCoin(x, y, amount);
            area.AddItem(coin);
        }
    }
}
