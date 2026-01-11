using Erg.Core.World.Items;

namespace Erg.Core.World.Generators;

public class DungeonGenerator3 : IDungeonGenerator
{
    private readonly int _width;
    private readonly int _height;
    private readonly Random _random;

    private int _nextRegionId = 1;
    private readonly List<RoomInfo> _rooms = new();
    private readonly List<CorridorInfo> _corridors = new();
    private readonly HashSet<(int, int)> _floorTiles = new();
    private (int x, int y) _playerStart;

    // Room size constraints
    private const int MinRoomWidth = 4;
    private const int MaxRoomWidth = 9;
    private const int MinRoomHeight = 4;
    private const int MaxRoomHeight = 7;

    // Room placement
    private const int RoomMargin = 3;  // Minimum walls between rooms
    private const int EdgeMargin = 2;  // Minimum distance from map edges
    private const int MaxConsecutiveFails = 100;

    public DungeonGenerator3(int width, int height, Random random)
    {
        _width = width;
        _height = height;
        _random = random;
    }

    public Area Generate()
    {
        var area = new Area(_width, _height);

        // Fill with rock
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                area.SetTile(x, y, Tile.Rock);

        // Generate rooms
        GenerateRooms(area);

        // Connect rooms with corridors
        ConnectRooms(area);

        // Remove disconnected rooms
        RemoveDisconnectedRooms(area);

        // Process doors
        ProcessDoors(area);

        // Place items in rooms
        PlaceItems(area);

        // Set player start in a connected room
        var connectedRoom = _rooms.FirstOrDefault(r => r.Connected) ?? _rooms.FirstOrDefault();
        if (connectedRoom != null)
        {
            _playerStart = (connectedRoom.CenterX, connectedRoom.CenterY);
        }

        return area;
    }

    private void ConnectRooms(Area area)
    {
        if (_rooms.Count < 2) return;

        int consecutiveFails = 0;
        const int maxFails = 500;

        while (consecutiveFails < maxFails)
        {
            // Stop if all rooms are connected
            if (_rooms.All(r => r.Connected))
                break;

            var result = TryCreateCorridor(area);
            if (result.Success)
                consecutiveFails = 0;
            else
                consecutiveFails++;
        }
    }

    public (int x, int y) GetPlayerStartPosition() => _playerStart;

    /// <summary>
    /// Step-by-step generation for debugging.
    /// </summary>
    public IEnumerable<GenerationStep> GenerateStepByStep()
    {
        var area = new Area(_width, _height);

        // Fill with rock
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                area.SetTile(x, y, Tile.Rock);
        yield return new GenerationStep("Wypełniono skałą", area);

        // Generate rooms with step-by-step feedback
        foreach (var step in GenerateRoomsStepByStep(area))
        {
            yield return step;
        }

        yield return new GenerationStep($"Etap 1 zakończony: {_rooms.Count} pokoi", area);

        // Phase 2: Connect rooms with corridors
        foreach (var step in ConnectRoomsStepByStep(area))
        {
            yield return step;
        }

        // Phase 4: Process doors
        ProcessDoors(area);
        yield return new GenerationStep("Etap 4 zakończony: przetworzono drzwi", area);

        // Phase 5: Place items
        PlaceItems(area);
        yield return new GenerationStep("Etap 5 zakończony: rozmieszczono przedmioty", area);

        // Set player start in a connected room
        var connectedRoom = _rooms.FirstOrDefault(r => r.Connected) ?? _rooms.FirstOrDefault();
        if (connectedRoom != null)
        {
            _playerStart = (connectedRoom.CenterX, connectedRoom.CenterY);
        }
    }

    #region Phase 1: Room Generation

    private void GenerateRooms(Area area)
    {
        int consecutiveFails = 0;

        while (consecutiveFails < MaxConsecutiveFails)
        {
            if (TryPlaceRoom(area))
            {
                consecutiveFails = 0;  // Reset on success
            }
            else
            {
                consecutiveFails++;
            }
        }
    }

    private IEnumerable<GenerationStep> GenerateRoomsStepByStep(Area area)
    {
        int consecutiveFails = 0;

        while (consecutiveFails < MaxConsecutiveFails)
        {
            var result = TryPlaceRoomWithDetails(area);

            if (result.Success)
            {
                consecutiveFails = 0;
                yield return new GenerationStep(
                    $"Pokój {result.Room!.Index}: ({result.Room.X},{result.Room.Y}) {result.Room.Width}x{result.Room.Height}",
                    area);
            }
            else
            {
                consecutiveFails++;
                // Don't yield on every fail - too spammy
                // Only report milestone fails
                if (consecutiveFails == 50)
                {
                    yield return new GenerationStep($"50 prób nieudanych...", area);
                }
            }
        }

        yield return new GenerationStep($"Zakończono po {MaxConsecutiveFails} nieudanych próbach", area);
    }

    private bool TryPlaceRoom(Area area)
    {
        return TryPlaceRoomWithDetails(area).Success;
    }

    private (bool Success, RoomInfo? Room) TryPlaceRoomWithDetails(Area area)
    {
        // Always start with maximum size, shrink if needed
        int roomWidth = MaxRoomWidth;
        int roomHeight = MaxRoomHeight;

        // Random position (must leave space for edge margin)
        int minX = EdgeMargin;
        int minY = EdgeMargin;
        int maxX = _width - MinRoomWidth - EdgeMargin;
        int maxY = _height - MinRoomHeight - EdgeMargin;

        if (maxX < minX || maxY < minY)
            return (false, null);

        int roomX = _random.Next(minX, maxX + 1);
        int roomY = _random.Next(minY, maxY + 1);

        // Try to fit room at this position, shrinking if necessary
        bool shrinkWidth = true;  // Alternate between shrinking width and height

        while (roomWidth >= MinRoomWidth && roomHeight >= MinRoomHeight)
        {
            // Check if room fits at this position with margin
            if (CanPlaceRoom(area, roomX, roomY, roomWidth, roomHeight))
            {
                // Place the room!
                var room = CarveRoom(area, roomX, roomY, roomWidth, roomHeight);
                return (true, room);
            }

            // Shrink room (alternate width/height)
            if (shrinkWidth)
            {
                roomWidth--;
                // Clamp to available space
                if (roomX + roomWidth > _width - EdgeMargin)
                    roomWidth = _width - EdgeMargin - roomX;
            }
            else
            {
                roomHeight--;
                // Clamp to available space
                if (roomY + roomHeight > _height - EdgeMargin)
                    roomHeight = _height - EdgeMargin - roomY;
            }
            shrinkWidth = !shrinkWidth;
        }

        // Couldn't fit even minimum size
        return (false, null);
    }

    private bool CanPlaceRoom(Area area, int roomX, int roomY, int roomWidth, int roomHeight)
    {
        // 1. Check map bounds - EdgeMargin tiles from edges
        if (roomX < EdgeMargin || roomY < EdgeMargin ||
            roomX + roomWidth > _width - EdgeMargin || roomY + roomHeight > _height - EdgeMargin)
            return false;

        // 2. Check room area + RoomMargin for other floors (rooms)
        //    But clamp to map bounds (edges don't need full margin)
        int checkMinX = Math.Max(0, roomX - RoomMargin);
        int checkMinY = Math.Max(0, roomY - RoomMargin);
        int checkMaxX = Math.Min(_width - 1, roomX + roomWidth + RoomMargin - 1);
        int checkMaxY = Math.Min(_height - 1, roomY + roomHeight + RoomMargin - 1);

        // Check that entire area is walls only (no existing floors)
        for (int y = checkMinY; y <= checkMaxY; y++)
        {
            for (int x = checkMinX; x <= checkMaxX; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile == null || tile.Walkable)  // Not a wall = existing room
                    return false;
            }
        }

        return true;
    }

    private RoomInfo CarveRoom(Area area, int roomX, int roomY, int roomWidth, int roomHeight)
    {
        int regionId = _nextRegionId++;
        var room = new RoomInfo(_rooms.Count, roomX, roomY, roomWidth, roomHeight, regionId);
        _rooms.Add(room);

        // Carve room floor
        for (int y = roomY; y < roomY + roomHeight; y++)
        {
            for (int x = roomX; x < roomX + roomWidth; x++)
            {
                var tile = Tile.Floor(TileStructure.Room);
                tile.RegionId = regionId;
                area.SetTile(x, y, tile);
                _floorTiles.Add((x, y));
            }
        }

        return room;
    }

    #endregion

    #region Phase 2: Corridor Generation

    private IEnumerable<GenerationStep> ConnectRoomsStepByStep(Area area)
    {
        if (_rooms.Count < 2)
        {
            yield return new GenerationStep("Za mało pokoi do połączenia", area);
            yield break;
        }

        int consecutiveFails = 0;
        const int maxFails = 100;

        while (consecutiveFails < maxFails)
        {
            // Stop if all rooms are connected
            if (_rooms.All(r => r.Connected))
                break;

            var result = TryCreateCorridor(area);

            if (result.Success)
            {
                consecutiveFails = 0;
                var start = result.Path![0];
                var end = result.Path[^1];
                string targetType = result.TargetType == "room" ? $"pokój {result.TargetIndex}" : $"korytarz {result.TargetIndex}";
                yield return new GenerationStep(
                    $"Korytarz {_corridors.Count - 1}: pokój {result.SourceRoomIndex}→{targetType} ({start.x},{start.y})→({end.x},{end.y}) dł.{result.Path.Count}",
                    area);
            }
            else
            {
                consecutiveFails++;
            }
        }

        int connectedRooms = _rooms.Count(r => r.Connected);
        int totalRooms = _rooms.Count;
        string status = connectedRooms == totalRooms ? "wszystkie połączone" : $"{connectedRooms}/{totalRooms} pokoi połączonych";
        yield return new GenerationStep($"Etap 2 zakończony: {_corridors.Count} korytarzy, {status}", area);

        // Phase 3: Remove disconnected rooms
        int removedCount = RemoveDisconnectedRooms(area);
        if (removedCount > 0)
        {
            yield return new GenerationStep($"Etap 3 zakończony: usunięto {removedCount} niepołączonych pokoi", area);
        }
        else
        {
            yield return new GenerationStep("Etap 3 zakończony: brak niepołączonych pokoi", area);
        }
    }

    private (bool Success, int SourceRoomIndex, string TargetType, int TargetIndex, List<(int x, int y)>? Path) TryCreateCorridor(Area area)
    {
        // Get connected rooms that haven't reached max doors
        var connectedRooms = _rooms.Where(r => r.Connected && r.DoorCount < RoomInfo.MaxDoors).ToList();

        // If no connected rooms yet, pick any room that hasn't reached max doors
        RoomInfo sourceRoom;
        if (connectedRooms.Count == 0)
        {
            var availableRooms = _rooms.Where(r => r.DoorCount < RoomInfo.MaxDoors).ToList();
            if (availableRooms.Count == 0)
                return (false, -1, "", -1, null);
            sourceRoom = availableRooms[_random.Next(availableRooms.Count)];
        }
        else
        {
            sourceRoom = connectedRooms[_random.Next(connectedRooms.Count)];
        }

        // Get random VALID wall candidate (not corner, not adjacent to doors)
        var candidate = GetRandomValidWallCandidate(sourceRoom, area);
        if (candidate == null)
            return (false, -1, "", -1, null);

        var (startX, startY, dx, dy) = candidate.Value;

        // Try to dig corridor
        var path = new List<(int x, int y)>();
        int x = startX;
        int y = startY;

        while (true)
        {
            // Check bounds
            if (x < 1 || x >= _width - 1 || y < 1 || y >= _height - 1)
                return (false, -1, "", -1, null);

            var tile = area.GetTile(x, y);

            // If we hit floor, check what it is
            if (tile != null && tile.Walkable)
            {
                // Don't connect back to source room
                if (tile.RegionId == sourceRoom.RegionId)
                    return (false, -1, "", -1, null);

                // Check if it's a room
                int targetRoomIndex = _rooms.FindIndex(r => r.RegionId == tile.RegionId);
                // Check if it's a corridor
                int targetCorridorIndex = _corridors.FindIndex(c => c.RegionId == tile.RegionId);

                if (targetRoomIndex >= 0 || targetCorridorIndex >= 0)
                {
                    // Success! Found valid target
                    if (path.Count < 3)
                        return (false, -1, "", -1, null);  // Too short (min 3: door-corridor-door)

                    // Carve corridor floor (middle part only, skip first and last for doors)
                    int corridorRegionId = _nextRegionId++;
                    var corridorTiles = new List<(int x, int y)>();

                    for (int i = 1; i < path.Count - 1; i++)
                    {
                        var (px, py) = path[i];
                        var corridorTile = Tile.Floor(TileStructure.Corridor);
                        corridorTile.RegionId = corridorRegionId;
                        area.SetTile(px, py, corridorTile);
                        _floorTiles.Add((px, py));
                        corridorTiles.Add((px, py));
                    }

                    // Place door at START (adjacent to source room)
                    var (startDoorX, startDoorY) = path[0];
                    PlaceDoor(area, startDoorX, startDoorY);

                    // Place door at END (adjacent to target)
                    var (endDoorX, endDoorY) = path[^1];
                    PlaceDoor(area, endDoorX, endDoorY);

                    // Create corridor info
                    var corridor = new CorridorInfo(_corridors.Count, corridorRegionId, corridorTiles);
                    corridor.Connected = true;
                    _corridors.Add(corridor);

                    // Mark source room as connected and increment door count
                    sourceRoom.Connected = true;
                    sourceRoom.DoorCount++;

                    // Mark target as connected
                    string targetType;
                    int targetIndex;
                    if (targetRoomIndex >= 0)
                    {
                        _rooms[targetRoomIndex].Connected = true;
                        _rooms[targetRoomIndex].DoorCount++;
                        targetType = "room";
                        targetIndex = targetRoomIndex;
                    }
                    else
                    {
                        _corridors[targetCorridorIndex].Connected = true;
                        targetType = "corridor";
                        targetIndex = targetCorridorIndex;
                    }

                    return (true, sourceRoom.Index, targetType, targetIndex, path);
                }
                else
                {
                    // Unknown floor - fail
                    return (false, -1, "", -1, null);
                }
            }

            // Check for diagonal floor (corner situation)
            if (dx != 0)  // Moving horizontally
            {
                if (HasFloorAt(area, x, y - 1) || HasFloorAt(area, x, y + 1))
                    return (false, -1, "", -1, null);
            }
            else  // Moving vertically
            {
                if (HasFloorAt(area, x - 1, y) || HasFloorAt(area, x + 1, y))
                    return (false, -1, "", -1, null);
            }

            // Add to path
            path.Add((x, y));

            // Move to next position
            x += dx;
            y += dy;

            // Safety limit
            if (path.Count > 50)
                return (false, -1, "", -1, null);
        }
    }

    private (int x, int y, int dx, int dy)? GetRandomValidWallCandidate(RoomInfo room, Area area)
    {
        var candidates = new List<(int x, int y, int dx, int dy)>();

        // Top wall (excluding corners)
        for (int x = room.X + 1; x < room.X + room.Width - 1; x++)
        {
            int wallY = room.Y - 1;
            if (wallY >= 1 && IsValidWallPosition(area, x, wallY))
                candidates.Add((x, wallY, 0, -1));
        }

        // Bottom wall
        for (int x = room.X + 1; x < room.X + room.Width - 1; x++)
        {
            int wallY = room.Y + room.Height;
            if (wallY < _height - 1 && IsValidWallPosition(area, x, wallY))
                candidates.Add((x, wallY, 0, 1));
        }

        // Left wall (excluding corners)
        for (int y = room.Y + 1; y < room.Y + room.Height - 1; y++)
        {
            int wallX = room.X - 1;
            if (wallX >= 1 && IsValidWallPosition(area, wallX, y))
                candidates.Add((wallX, y, -1, 0));
        }

        // Right wall
        for (int y = room.Y + 1; y < room.Y + room.Height - 1; y++)
        {
            int wallX = room.X + room.Width;
            if (wallX < _width - 1 && IsValidWallPosition(area, wallX, y))
                candidates.Add((wallX, y, 1, 0));
        }

        if (candidates.Count == 0)
            return null;

        return candidates[_random.Next(candidates.Count)];
    }

    private bool IsValidWallPosition(Area area, int x, int y)
    {
        // Must be a wall (not already floor/door)
        if (HasFloorAt(area, x, y))
            return false;

        // Must not be within 2 tiles of existing doors (horizontally or vertically)
        for (int dist = 1; dist <= 2; dist++)
        {
            if (IsDoorAt(area, x - dist, y) || IsDoorAt(area, x + dist, y) ||
                IsDoorAt(area, x, y - dist) || IsDoorAt(area, x, y + dist))
                return false;
        }

        return true;
    }

    private bool HasFloorAt(Area area, int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return false;
        var tile = area.GetTile(x, y);
        return tile != null && tile.Walkable;
    }

    private bool IsDoorAt(Area area, int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return false;
        var tile = area.GetTile(x, y);
        return tile != null && tile.Type is TileType.OpenDoor or TileType.ClosedDoor;
    }

    private void PlaceDoor(Area area, int x, int y)
    {
        var door = Tile.ClosedDoor;
        door.RegionId = _nextRegionId++;
        area.SetTile(x, y, door);
        _floorTiles.Add((x, y));
    }

    #endregion

    #region Phase 3: Remove Disconnected Rooms

    private int RemoveDisconnectedRooms(Area area)
    {
        var disconnectedRooms = _rooms.Where(r => !r.Connected).ToList();

        foreach (var room in disconnectedRooms)
        {
            // Fill room tiles with Rock
            for (int y = room.Y; y < room.Y + room.Height; y++)
            {
                for (int x = room.X; x < room.X + room.Width; x++)
                {
                    area.SetTile(x, y, Tile.Rock);
                    _floorTiles.Remove((x, y));
                }
            }

            // Remove from rooms list
            _rooms.Remove(room);
        }

        return disconnectedRooms.Count;
    }

    #endregion

    #region Phase 4: Door Processing

    private void ProcessDoors(Area area)
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile == null || !IsDoorTile(tile)) continue;

                bool allAdjacentFloorsAreCorridor = CheckAllAdjacentFloorsAreCorridor(area, x, y);

                if (allAdjacentFloorsAreCorridor)
                {
                    // Door between corridors - replace with Entrance floor
                    var entrance = Tile.Floor(TileStructure.Entrance);
                    entrance.RegionId = tile.RegionId;
                    area.SetTile(x, y, entrance);
                }
                else
                {
                    // Door at room entrance - randomize type
                    int roll = _random.Next(100);
                    Tile newTile;
                    if (roll < 30)
                        newTile = Tile.OpenDoor;
                    else if (roll < 96)  // 30 + 66 = 96
                        newTile = Tile.ClosedDoor;
                    else  // 4%
                        newTile = Tile.SecretDoor;

                    newTile.RegionId = tile.RegionId;
                    area.SetTile(x, y, newTile);
                }
            }
        }
    }

    private bool CheckAllAdjacentFloorsAreCorridor(Area area, int x, int y)
    {
        var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

        foreach (var (dx, dy) in directions)
        {
            int nx = x + dx;
            int ny = y + dy;
            if (nx < 0 || nx >= _width || ny < 0 || ny >= _height)
                continue;

            var neighbor = area.GetTile(nx, ny);
            if (neighbor != null && neighbor.Walkable)
            {
                int regionId = neighbor.RegionId;
                bool isRoom = _rooms.Any(r => r.RegionId == regionId);
                if (isRoom)
                    return false;  // Found room floor
            }
        }
        return true;  // All adjacent floors are corridors
    }

    private bool IsDoorTile(Tile tile)
    {
        return tile.Type is TileType.OpenDoor or TileType.ClosedDoor;
    }

    #endregion

    #region Phase 5: Item Placement

    private void PlaceItems(Area area)
    {
        // Collect all room floor tiles
        var roomFloors = new List<(int x, int y)>();
        foreach (var room in _rooms)
        {
            for (int y = room.Y; y < room.Y + room.Height; y++)
            {
                for (int x = room.X; x < room.X + room.Width; x++)
                {
                    roomFloors.Add((x, y));
                }
            }
        }

        if (roomFloors.Count == 0) return;

        // Place 2-10 items
        int itemCount = _random.Next(2, 11);
        for (int i = 0; i < itemCount && roomFloors.Count > 0; i++)
        {
            // Pick random floor tile
            int index = _random.Next(roomFloors.Count);
            var (x, y) = roomFloors[index];
            roomFloors.RemoveAt(index);  // Don't place multiple items on same tile

            // Create Gold Coin with random count 1-100
            int goldAmount = _random.Next(1, 101);
            var coin = new GoldCoin(x, y, goldAmount);
            area.AddItem(coin);
        }
    }

    #endregion

    #region Helper Classes

    private class RoomInfo
    {
        public int Index;
        public int X, Y, Width, Height;
        public int RegionId;
        public bool Connected;  // True when room is connected to dungeon network
        public int DoorCount;   // Number of doors/corridors connected to this room

        public int CenterX => X + Width / 2;
        public int CenterY => Y + Height / 2;

        public const int MaxDoors = 3;

        public RoomInfo(int index, int x, int y, int width, int height, int regionId)
        {
            Index = index;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            RegionId = regionId;
            Connected = false;
            DoorCount = 0;
        }
    }

    private class CorridorInfo
    {
        public int Index;
        public int RegionId;
        public List<(int x, int y)> Tiles;
        public bool Connected;

        public CorridorInfo(int index, int regionId, List<(int x, int y)> tiles)
        {
            Index = index;
            RegionId = regionId;
            Tiles = tiles;
            Connected = false;
        }
    }

    #endregion
}
