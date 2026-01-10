using Erg.Core.World.Items;

namespace Erg.Core.World.Generators;

public class GenerationStep
{
    public string Message { get; }
    public Area Area { get; }

    public GenerationStep(string message, Area area)
    {
        Message = message;
        Area = area;
    }
}

public class DungeonGenerator2 : IDungeonGenerator
{
    private readonly int _width;
    private readonly int _height;
    private readonly Random _random;

    private int _nextRegionId = 1;
    private readonly List<RoomInfo> _rooms = new();
    private readonly HashSet<(int, int)> _floorTiles = new();
    private readonly HashSet<(int roomA, int roomB)> _connections = new();
    private (int x, int y) _playerStart;

    // Configuration
    private const int MinPartitionSize = 9;   // Max 9 to allow vertical splits on 20-height map
    private const int MinRoomWidth = 4;
    private const int MinRoomHeight = 3;
    private const int MaxRoomWidth = 12;
    private const int MaxRoomHeight = 6;
    private const int RoomMargin = 2;         // Larger = more space between rooms
    private const double ExtraConnectionChance = 0.10;  // Reduced from 0.30
    private const int MaxExtraConnectionDist = 15;      // Reduced from 25
    private const int DeadEndMinCount = 1;              // Reduced from 3
    private const int DeadEndMaxCount = 3;              // Reduced from 6
    private const int DeadEndMinLength = 2;             // Reduced from 3
    private const int DeadEndMaxLength = 5;             // Reduced from 8

    public DungeonGenerator2(int width, int height, Random random)
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
            for (int x = 0; x < _width; x++)
                area.SetTile(x, y, Tile.DungeonWall);

        // Phase 1-2: BSP partition and create rooms
        var root = new BspNode(1, 1, _width - 2, _height - 2);
        Partition(root);
        CreateRooms(root, area);

        // Phase 3: Connect rooms via BSP tree
        ConnectRooms(root, area);

        // Phase 4: Add extra connections (loops)
        AddExtraConnections(area);

        // Phase 5: Add dead ends
        AddDeadEnds(area);

        // Phase 6: Clean up orphan doors
        CleanupOrphanDoors(area);

        // Phase 7: Place items (disabled for debugging)
        // PlaceItems(area);

        // Set player start
        if (_rooms.Count > 0)
        {
            var startRoom = _rooms[_random.Next(_rooms.Count)];
            _playerStart = (startRoom.CenterX, startRoom.CenterY);
        }

        return area;
    }

    public (int x, int y) GetPlayerStartPosition() => _playerStart;

    /// <summary>
    /// Step-by-step generation for debugging. Each yield pauses generation.
    /// </summary>
    public IEnumerable<GenerationStep> GenerateStepByStep()
    {
        var area = new Area(_width, _height);

        // Fill with walls
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                area.SetTile(x, y, Tile.DungeonWall);
        yield return new GenerationStep("Wypełniono ścianami", area);

        // Phase 1-2: BSP partition and create rooms
        var root = new BspNode(1, 1, _width - 2, _height - 2);
        Partition(root);
        yield return new GenerationStep("Partycjonowanie BSP zakończone", area);

        // Create rooms one by one
        foreach (var step in CreateRoomsStepByStep(root, area))
        {
            yield return step;
        }

        // Phase 3: Connect rooms via BSP tree
        foreach (var step in ConnectRoomsStepByStep(root, area))
        {
            yield return step;
        }

        // Phase 4: Add extra connections (loops) - skip for now in debug
        // AddExtraConnections(area);

        // Phase 5: Add dead ends - skip for now in debug
        // AddDeadEnds(area);

        // Set player start
        if (_rooms.Count > 0)
        {
            var startRoom = _rooms[_random.Next(_rooms.Count)];
            _playerStart = (startRoom.CenterX, startRoom.CenterY);
        }

        yield return new GenerationStep("Generacja zakończona!", area);
    }

    private IEnumerable<GenerationStep> CreateRoomsStepByStep(BspNode node, Area area)
    {
        if (node.IsLeaf)
        {
            int maxWidth = Math.Min(node.Width - RoomMargin * 2, MaxRoomWidth);
            int maxHeight = Math.Min(node.Height - RoomMargin * 2, MaxRoomHeight);

            if (maxWidth >= MinRoomWidth && maxHeight >= MinRoomHeight)
            {
                int roomWidth = _random.Next(MinRoomWidth, maxWidth + 1);
                int roomHeight = _random.Next(MinRoomHeight, maxHeight + 1);

                int roomX = node.X + RoomMargin + _random.Next(maxWidth - roomWidth + 1);
                int roomY = node.Y + RoomMargin + _random.Next(maxHeight - roomHeight + 1);

                int regionId = _nextRegionId++;
                var room = new RoomInfo(_rooms.Count, roomX, roomY, roomWidth, roomHeight, regionId);
                node.Room = room;
                _rooms.Add(room);

                // Carve room
                for (int y = roomY; y < roomY + roomHeight; y++)
                {
                    for (int x = roomX; x < roomX + roomWidth; x++)
                    {
                        var tile = Tile.RoomFloor;
                        tile.RegionId = regionId;
                        area.SetTile(x, y, tile);
                        _floorTiles.Add((x, y));
                    }
                }

                yield return new GenerationStep($"Pokój {room.Index} w ({roomX},{roomY}) {roomWidth}x{roomHeight}", area);
            }
        }
        else
        {
            if (node.Left != null)
            {
                foreach (var step in CreateRoomsStepByStep(node.Left, area))
                    yield return step;
            }
            if (node.Right != null)
            {
                foreach (var step in CreateRoomsStepByStep(node.Right, area))
                    yield return step;
            }
        }
    }

    private IEnumerable<GenerationStep> ConnectRoomsStepByStep(BspNode node, Area area)
    {
        if (node.IsLeaf)
            yield break;

        RoomInfo? leftRoom = null;
        RoomInfo? rightRoom = null;

        if (node.Left != null)
        {
            foreach (var step in ConnectRoomsStepByStep(node.Left, area))
                yield return step;
            leftRoom = GetRoomFromNode(node.Left);
        }

        if (node.Right != null)
        {
            foreach (var step in ConnectRoomsStepByStep(node.Right, area))
                yield return step;
            rightRoom = GetRoomFromNode(node.Right);
        }

        if (leftRoom != null && rightRoom != null)
        {
            foreach (var step in CreateCorridorStepByStep(area, leftRoom, rightRoom))
                yield return step;

            _connections.Add((Math.Min(leftRoom.Index, rightRoom.Index), Math.Max(leftRoom.Index, rightRoom.Index)));
        }
    }

    private RoomInfo? GetRoomFromNode(BspNode node)
    {
        if (node.IsLeaf)
            return node.Room;

        var leftRoom = node.Left != null ? GetRoomFromNode(node.Left) : null;
        var rightRoom = node.Right != null ? GetRoomFromNode(node.Right) : null;

        if (leftRoom != null && rightRoom != null)
            return _random.Next(2) == 0 ? leftRoom : rightRoom;
        return leftRoom ?? rightRoom;
    }

    private IEnumerable<GenerationStep> CreateCorridorStepByStep(Area area, RoomInfo roomA, RoomInfo roomB)
    {
        var (exitX, exitY, exitDirX, _) = GetExitPoint(roomA, roomB.CenterX, roomB.CenterY);
        var (entryX, entryY, _, _) = GetExitPoint(roomB, roomA.CenterX, roomA.CenterY);

        int distance = Math.Abs(exitX - entryX) + Math.Abs(exitY - entryY);
        if (distance < 2)
            yield break;

        yield return new GenerationStep($"Korytarz {roomA.Index}→{roomB.Index}: exit({exitX},{exitY}) entry({entryX},{entryY})", area);

        int corridorRegionId = _nextRegionId++;

        // Carve L-shaped corridor - first segment follows exit direction (AWAY from room)
        if (exitDirX != 0)
        {
            // Exit is horizontal - carve horizontal first
            CarveCorridorLine(area, exitX, exitY, entryX, exitY, corridorRegionId);
            yield return new GenerationStep($"  Segment poziomy ({exitX},{exitY})→({entryX},{exitY})", area);

            CarveCorridorLine(area, entryX, exitY, entryX, entryY, corridorRegionId);
            yield return new GenerationStep($"  Segment pionowy ({entryX},{exitY})→({entryX},{entryY})", area);
        }
        else
        {
            // Exit is vertical - carve vertical first
            CarveCorridorLine(area, exitX, exitY, exitX, entryY, corridorRegionId);
            yield return new GenerationStep($"  Segment pionowy ({exitX},{exitY})→({exitX},{entryY})", area);

            CarveCorridorLine(area, exitX, entryY, entryX, entryY, corridorRegionId);
            yield return new GenerationStep($"  Segment poziomy ({exitX},{entryY})→({entryX},{entryY})", area);
        }

        PlaceDoor(area, exitX, exitY, Tile.OpenDoor);
        PlaceDoor(area, entryX, entryY, Tile.ClosedDoor);
        yield return new GenerationStep($"  Drzwi: exit=/ entry=+", area);
    }

    #region Phase 1: BSP Partitioning

    private void Partition(BspNode node)
    {
        // Stop if too small to split
        if (node.Width < MinPartitionSize * 2 && node.Height < MinPartitionSize * 2)
            return;

        // Decide split direction
        bool splitHorizontal;
        if (node.Width < MinPartitionSize * 2)
            splitHorizontal = true;
        else if (node.Height < MinPartitionSize * 2)
            splitHorizontal = false;
        else
            splitHorizontal = _random.Next(2) == 0;

        if (splitHorizontal)
        {
            if (node.Height < MinPartitionSize * 2) return;

            // Split between 30%-70%
            int minSplit = node.Y + (int)(node.Height * 0.3);
            int maxSplit = node.Y + (int)(node.Height * 0.7);
            if (maxSplit <= minSplit) return;

            int splitY = _random.Next(minSplit, maxSplit);

            node.Left = new BspNode(node.X, node.Y, node.Width, splitY - node.Y);
            node.Right = new BspNode(node.X, splitY, node.Width, node.Y + node.Height - splitY);
        }
        else
        {
            if (node.Width < MinPartitionSize * 2) return;

            int minSplit = node.X + (int)(node.Width * 0.3);
            int maxSplit = node.X + (int)(node.Width * 0.7);
            if (maxSplit <= minSplit) return;

            int splitX = _random.Next(minSplit, maxSplit);

            node.Left = new BspNode(node.X, node.Y, splitX - node.X, node.Height);
            node.Right = new BspNode(splitX, node.Y, node.X + node.Width - splitX, node.Height);
        }

        Partition(node.Left);
        Partition(node.Right);
    }

    #endregion

    #region Phase 2: Create Rooms

    private void CreateRooms(BspNode node, Area area)
    {
        if (node.IsLeaf)
        {
            // Create room in this leaf
            int maxWidth = Math.Min(node.Width - RoomMargin * 2, MaxRoomWidth);
            int maxHeight = Math.Min(node.Height - RoomMargin * 2, MaxRoomHeight);

            if (maxWidth < MinRoomWidth || maxHeight < MinRoomHeight)
                return;

            int roomWidth = _random.Next(MinRoomWidth, maxWidth + 1);
            int roomHeight = _random.Next(MinRoomHeight, maxHeight + 1);

            int roomX = node.X + RoomMargin + _random.Next(maxWidth - roomWidth + 1);
            int roomY = node.Y + RoomMargin + _random.Next(maxHeight - roomHeight + 1);

            int regionId = _nextRegionId++;
            var room = new RoomInfo(_rooms.Count, roomX, roomY, roomWidth, roomHeight, regionId);
            node.Room = room;
            _rooms.Add(room);

            // Carve room
            for (int y = roomY; y < roomY + roomHeight; y++)
            {
                for (int x = roomX; x < roomX + roomWidth; x++)
                {
                    var tile = Tile.RoomFloor;  // RED for rooms
                    tile.RegionId = regionId;
                    area.SetTile(x, y, tile);
                    _floorTiles.Add((x, y));
                }
            }
        }
        else
        {
            if (node.Left != null) CreateRooms(node.Left, area);
            if (node.Right != null) CreateRooms(node.Right, area);
        }
    }

    #endregion

    #region Phase 3: Connect Rooms (BSP Tree)

    private RoomInfo? ConnectRooms(BspNode node, Area area)
    {
        if (node.IsLeaf)
            return node.Room;

        RoomInfo? leftRoom = node.Left != null ? ConnectRooms(node.Left, area) : null;
        RoomInfo? rightRoom = node.Right != null ? ConnectRooms(node.Right, area) : null;

        if (leftRoom != null && rightRoom != null)
        {
            CreateCorridor(area, leftRoom, rightRoom);
            _connections.Add((Math.Min(leftRoom.Index, rightRoom.Index), Math.Max(leftRoom.Index, rightRoom.Index)));
        }

        // Return one of the rooms as representative
        if (leftRoom != null && rightRoom != null)
            return _random.Next(2) == 0 ? leftRoom : rightRoom;
        return leftRoom ?? rightRoom;
    }

    private void CreateCorridor(Area area, RoomInfo roomA, RoomInfo roomB)
    {
        // Find exit point from room A (just outside the room)
        var (exitX, exitY, exitDirX, exitDirY) = GetExitPoint(roomA, roomB.CenterX, roomB.CenterY);
        // Find entry point to room B (just outside the room)
        var (entryX, entryY, _, _) = GetExitPoint(roomB, roomA.CenterX, roomA.CenterY);

        // Calculate Manhattan distance between door positions
        int distance = Math.Abs(exitX - entryX) + Math.Abs(exitY - entryY);

        // Skip corridor if rooms are adjacent (distance < 2)
        if (distance < 2)
            return;

        int corridorRegionId = _nextRegionId++;

        // Carve L-shaped corridor - first segment follows exit direction (AWAY from room)
        if (exitDirX != 0)
        {
            // Exit is horizontal - carve horizontal first
            CarveCorridorLine(area, exitX, exitY, entryX, exitY, corridorRegionId);
            CarveCorridorLine(area, entryX, exitY, entryX, entryY, corridorRegionId);
        }
        else
        {
            // Exit is vertical - carve vertical first
            CarveCorridorLine(area, exitX, exitY, exitX, entryY, corridorRegionId);
            CarveCorridorLine(area, exitX, entryY, entryX, entryY, corridorRegionId);
        }

        // Place doors AFTER carving (so they don't get overwritten)
        // DEBUG: Open door = exit (start), Closed door = entry (end)
        PlaceDoor(area, exitX, exitY, Tile.OpenDoor);
        PlaceDoor(area, entryX, entryY, Tile.ClosedDoor);
    }

    private (int x, int y, int dirX, int dirY) GetExitPoint(RoomInfo room, int targetX, int targetY)
    {
        // Determine which wall to exit from based on target direction
        int dx = targetX - room.CenterX;
        int dy = targetY - room.CenterY;

        // Choose wall based on dominant direction
        if (Math.Abs(dx) > Math.Abs(dy))
        {
            // Exit horizontally - avoid corners by staying 1 tile away from edges
            int minY = room.Y;
            int maxY = room.Y + room.Height - 1;
            if (room.Height >= 3)
            {
                minY = room.Y + 1;
                maxY = room.Y + room.Height - 2;
            }
            int y = Math.Clamp(targetY, minY, maxY);

            if (dx > 0)
                return (room.X + room.Width, y, 1, 0);
            else
                return (room.X - 1, y, -1, 0);
        }
        else
        {
            // Exit vertically - avoid corners by staying 1 tile away from edges
            int minX = room.X;
            int maxX = room.X + room.Width - 1;
            if (room.Width >= 3)
            {
                minX = room.X + 1;
                maxX = room.X + room.Width - 2;
            }
            int x = Math.Clamp(targetX, minX, maxX);

            if (dy > 0)
                return (x, room.Y + room.Height, 0, 1);
            else
                return (x, room.Y - 1, 0, -1);
        }
    }

    private void PlaceDoor(Area area, int x, int y, Tile doorType)
    {
        if (x <= 0 || x >= _width - 1 || y <= 0 || y >= _height - 1)
            return;

        var door = doorType;
        door.RegionId = _nextRegionId++;
        area.SetTile(x, y, door);
        _floorTiles.Add((x, y));
    }

    private void CarveCorridorLine(Area area, int x1, int y1, int x2, int y2, int regionId)
    {
        int dx = Math.Sign(x2 - x1);
        int dy = Math.Sign(y2 - y1);

        int x = x1;
        int y = y1;

        // Skip if starting position is already a door (don't overwrite it)
        var startTile = area.GetTile(x, y);
        if (startTile != null && IsDoor(startTile))
        {
            // Move to next position
            if (dx != 0) x += dx;
            else if (dy != 0) y += dy;
        }

        bool wasInRoom = IsInAnyRoom(x, y);
        bool skipCarving = false;

        while (true)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                bool nowInRoom = IsInAnyRoom(x, y);
                skipCarving = false;

                // Detect room boundary crossing - place doors only for PERPENDICULAR entry
                if (!wasInRoom && nowInRoom)
                {
                    // ENTERING room - check if entry is perpendicular
                    var room = GetRoomAt(x, y);
                    if (room != null)
                    {
                        bool isPerpendicularEntry = false;

                        if (dy != 0)  // Moving vertically
                        {
                            // Must enter through TOP or BOTTOM wall
                            // x must be in room's interior (not at left/right edge)
                            isPerpendicularEntry = x > room.X && x < room.X + room.Width - 1;
                        }
                        else if (dx != 0)  // Moving horizontally
                        {
                            // Must enter through LEFT or RIGHT wall
                            // y must be in room's interior (not at top/bottom edge)
                            isPerpendicularEntry = y > room.Y && y < room.Y + room.Height - 1;
                        }

                        if (isPerpendicularEntry)
                        {
                            // Valid entry - place door at wall position
                            int wallX = x - dx;
                            int wallY = y - dy;
                            PlaceDoor(area, wallX, wallY, Tile.ClosedDoor);
                        }
                        else
                        {
                            // Parallel/corner entry - STOP carving to avoid floor connection
                            break;
                        }
                    }
                }
                else if (wasInRoom && !nowInRoom)
                {
                    // EXITING room - check if exit is perpendicular
                    int prevX = x - dx;
                    int prevY = y - dy;
                    var room = GetRoomAt(prevX, prevY);
                    if (room != null)
                    {
                        bool isPerpendicularExit = false;

                        if (dy != 0)  // Moving vertically
                        {
                            isPerpendicularExit = prevX > room.X && prevX < room.X + room.Width - 1;
                        }
                        else if (dx != 0)  // Moving horizontally
                        {
                            isPerpendicularExit = prevY > room.Y && prevY < room.Y + room.Height - 1;
                        }

                        if (isPerpendicularExit)
                        {
                            // Valid exit - place door at current position (wall tile)
                            PlaceDoor(area, x, y, Tile.ClosedDoor);
                            skipCarving = true;  // Don't overwrite the door!
                        }
                        // Note: non-perpendicular exit from a room we entered perpendicularly is OK
                    }
                }

                var existingTile = area.GetTile(x, y);
                // Only carve if it's a wall (don't overwrite rooms or doors)
                if (!skipCarving && existingTile != null && !existingTile.Walkable)
                {
                    // Check if carving here would be adjacent to ANY floor (room or corridor)
                    // This prevents corridors from running alongside rooms or other corridors
                    bool hasAdjacentFloor = false;
                    if (dx != 0) // Moving horizontally - check above/below
                    {
                        hasAdjacentFloor = _floorTiles.Contains((x, y - 1)) || _floorTiles.Contains((x, y + 1));
                    }
                    else if (dy != 0) // Moving vertically - check left/right
                    {
                        hasAdjacentFloor = _floorTiles.Contains((x - 1, y)) || _floorTiles.Contains((x + 1, y));
                    }

                    if (!hasAdjacentFloor)
                    {
                        // Safe to carve - no adjacent floors
                        var tile = Tile.CorridorFloor;  // GREEN for corridors
                        tile.RegionId = regionId;
                        area.SetTile(x, y, tile);
                        _floorTiles.Add((x, y));
                    }
                    // else: would be adjacent to floor - skip this tile (leave as wall)
                }
                else if (existingTile != null && existingTile.Walkable)
                {
                    // Already floor - track it for path continuity
                    _floorTiles.Add((x, y));
                }

                wasInRoom = nowInRoom;
            }

            if (x == x2 && y == y2) break;

            if (dx != 0) x += dx;
            else if (dy != 0) y += dy;
            else break;
        }
    }

    #endregion

    #region Phase 4: Extra Connections (Loops)

    private void AddExtraConnections(Area area)
    {
        for (int i = 0; i < _rooms.Count; i++)
        {
            for (int j = i + 1; j < _rooms.Count; j++)
            {
                // Skip if already connected
                if (_connections.Contains((i, j)))
                    continue;

                var roomA = _rooms[i];
                var roomB = _rooms[j];

                // Calculate distance
                int dx = roomA.CenterX - roomB.CenterX;
                int dy = roomA.CenterY - roomB.CenterY;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist < MaxExtraConnectionDist && _random.NextDouble() < ExtraConnectionChance)
                {
                    CreateCorridor(area, roomA, roomB);
                    _connections.Add((i, j));
                }
            }
        }
    }

    #endregion

    #region Phase 5: Dead Ends

    private void AddDeadEnds(Area area)
    {
        int deadEndCount = _random.Next(DeadEndMinCount, DeadEndMaxCount + 1);

        // Find corridor tiles (floor tiles that aren't in rooms)
        var corridorTiles = _floorTiles
            .Where(pos => !IsInAnyRoom(pos.Item1, pos.Item2))
            .ToList();

        if (corridorTiles.Count == 0) return;

        for (int i = 0; i < deadEndCount && corridorTiles.Count > 0; i++)
        {
            var (startX, startY) = corridorTiles[_random.Next(corridorTiles.Count)];

            // Try each direction
            var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            Shuffle(directions);

            foreach (var (dx, dy) in directions)
            {
                if (TryCreateDeadEnd(area, startX, startY, dx, dy))
                    break;
            }
        }
    }

    private bool TryCreateDeadEnd(Area area, int startX, int startY, int dx, int dy)
    {
        int length = _random.Next(DeadEndMinLength, DeadEndMaxLength + 1);
        var positions = new List<(int x, int y)>();

        int x = startX + dx;
        int y = startY + dy;

        for (int i = 0; i < length; i++)
        {
            if (x <= 0 || x >= _width - 1 || y <= 0 || y >= _height - 1)
                return false;

            var tile = area.GetTile(x, y);
            if (tile == null || tile.Walkable) // Hit existing floor
                return false;

            // Check perpendicular tiles - don't carve next to existing floors
            if (dx != 0) // Moving horizontally - check above/below
            {
                if (_floorTiles.Contains((x, y - 1)) || _floorTiles.Contains((x, y + 1)))
                    return false;
            }
            else // Moving vertically - check left/right
            {
                if (_floorTiles.Contains((x - 1, y)) || _floorTiles.Contains((x + 1, y)))
                    return false;
            }

            positions.Add((x, y));
            x += dx;
            y += dy;
        }

        if (positions.Count < DeadEndMinLength)
            return false;

        // Carve the dead end
        int corridorRegionId = _nextRegionId++;
        foreach (var (px, py) in positions)
        {
            var tile = Tile.DeadEndFloor;  // BLUE for dead ends
            tile.RegionId = corridorRegionId;
            area.SetTile(px, py, tile);
            _floorTiles.Add((px, py));
        }

        // 50% chance: add small room at the end (but validate it first)
        if (_random.Next(2) == 0)
        {
            var (endX, endY) = positions[^1];
            TryCreateMiniRoom(area, endX + dx, endY + dy);
        }

        return true;
    }

    private void TryCreateMiniRoom(Area area, int x, int y)
    {
        int size = _random.Next(2, 4); // 2x2 or 3x3

        // First validate: check if any tile would be adjacent to existing floor
        for (int ry = y; ry < y + size && ry < _height - 1; ry++)
        {
            for (int rx = x; rx < x + size && rx < _width - 1; rx++)
            {
                if (rx <= 0 || ry <= 0) return;

                // Check all 8 neighbors for existing floors
                for (int ny = -1; ny <= 1; ny++)
                {
                    for (int nx = -1; nx <= 1; nx++)
                    {
                        if (nx == 0 && ny == 0) continue;
                        if (_floorTiles.Contains((rx + nx, ry + ny)))
                            return; // Would merge with existing floor, abort
                    }
                }
            }
        }

        // Safe to carve
        int regionId = _nextRegionId++;
        for (int ry = y; ry < y + size && ry < _height - 1; ry++)
        {
            for (int rx = x; rx < x + size && rx < _width - 1; rx++)
            {
                if (rx <= 0 || ry <= 0) continue;

                var existing = area.GetTile(rx, ry);
                if (existing != null && !existing.Walkable)
                {
                    var tile = Tile.DeadEndFloor;  // BLUE for mini rooms (dead end)
                    tile.RegionId = regionId;
                    area.SetTile(rx, ry, tile);
                    _floorTiles.Add((rx, ry));
                }
            }
        }
    }

    private bool IsInAnyRoom(int x, int y)
    {
        foreach (var room in _rooms)
        {
            if (x >= room.X && x < room.X + room.Width &&
                y >= room.Y && y < room.Y + room.Height)
                return true;
        }
        return false;
    }

    private RoomInfo? GetRoomAt(int x, int y)
    {
        foreach (var room in _rooms)
        {
            if (x >= room.X && x < room.X + room.Width &&
                y >= room.Y && y < room.Y + room.Height)
                return room;
        }
        return null;
    }

    private bool IsDoorAt(Area area, int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return false;
        var tile = area.GetTile(x, y);
        return tile != null && (tile.Name == "Open Door" || tile.Name == "Closed Door");
    }

    private void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    #endregion

    #region Phase 6: Cleanup

    private void CleanupOrphanDoors(Area area)
    {
        // DEBUG: All door cleanup passes disabled to diagnose missing doors
        // TODO: Re-enable after fixing door placement

        /*
        // Pass 1: Remove orphan doors (doors with <2 cardinal walkable neighbors)
        for (int y = 1; y < _height - 1; y++)
        {
            for (int x = 1; x < _width - 1; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile == null) continue;
                if (!IsDoor(tile)) continue;

                int walkableNeighbors = CountCardinalWalkable(area, x, y);

                if (walkableNeighbors < 2)
                {
                    if (walkableNeighbors >= 1)
                    {
                        var floor = Tile.DungeonFloor;
                        floor.RegionId = tile.RegionId;
                        area.SetTile(x, y, floor);
                    }
                    else
                    {
                        area.SetTile(x, y, Tile.DungeonWall);
                        _floorTiles.Remove((x, y));
                    }
                }
            }
        }

        // Pass 2: Remove diagonally adjacent doors (keep first, convert second to floor)
        for (int y = 1; y < _height - 1; y++)
        {
            for (int x = 1; x < _width - 1; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile == null || !IsDoor(tile)) continue;

                // Check diagonal neighbors for doors
                int[] diagX = { -1, 1, -1, 1 };
                int[] diagY = { -1, -1, 1, 1 };
                for (int i = 0; i < 4; i++)
                {
                    var diag = area.GetTile(x + diagX[i], y + diagY[i]);
                    if (diag != null && IsDoor(diag))
                    {
                        // Convert THIS door to floor (keep the diagonal one)
                        var floor = Tile.DungeonFloor;
                        floor.RegionId = tile.RegionId;
                        area.SetTile(x, y, floor);
                        break;
                    }
                }
            }
        }

        // Pass 3: Fix diagonal-only floor connections
        // If a floor tile connects to another floor ONLY diagonally, add a floor tile to make cardinal connection
        FixDiagonalConnections(area);

        // Pass 4: Validate door frames - doors must have walls on opposite sides
        for (int y = 1; y < _height - 1; y++)
        {
            for (int x = 1; x < _width - 1; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile == null || !IsDoor(tile)) continue;

                var left = area.GetTile(x - 1, y);
                var right = area.GetTile(x + 1, y);
                var top = area.GetTile(x, y - 1);
                var bottom = area.GetTile(x, y + 1);

                bool horizontalFrame = (left != null && !left.Walkable) &&
                                       (right != null && !right.Walkable);
                bool verticalFrame = (top != null && !top.Walkable) &&
                                     (bottom != null && !bottom.Walkable);

                // Door must have EITHER horizontal OR vertical wall frame
                if (!horizontalFrame && !verticalFrame)
                {
                    var floor = Tile.DungeonFloor;
                    floor.RegionId = tile.RegionId;
                    area.SetTile(x, y, floor);
                }
            }
        }
        */

        // All cleanup passes disabled - relying on proper validation during carving
    }

    private void FixDiagonalConnections(Area area)
    {
        var toFill = new List<(int x, int y)>();

        for (int y = 1; y < _height - 1; y++)
        {
            for (int x = 1; x < _width - 1; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile == null || !tile.Walkable) continue;

                // Check each diagonal
                // If diagonal is walkable but BOTH adjacent cardinals are walls,
                // we have a diagonal-only connection
                int[] diagX = { -1, 1, -1, 1 };
                int[] diagY = { -1, -1, 1, 1 };
                int[] adj1X = { -1, 1, -1, 1 }; // horizontal adjacent
                int[] adj1Y = { 0, 0, 0, 0 };
                int[] adj2X = { 0, 0, 0, 0 };   // vertical adjacent
                int[] adj2Y = { -1, -1, 1, 1 };

                for (int i = 0; i < 4; i++)
                {
                    var diag = area.GetTile(x + diagX[i], y + diagY[i]);
                    if (diag == null || !diag.Walkable) continue;

                    var adj1 = area.GetTile(x + adj1X[i], y + adj1Y[i]);
                    var adj2 = area.GetTile(x + adj2X[i], y + adj2Y[i]);

                    // If both cardinals between us and diagonal are walls, fill one
                    if (adj1 != null && !adj1.Walkable && adj2 != null && !adj2.Walkable)
                    {
                        // Fill the horizontal one (arbitrary choice)
                        toFill.Add((x + adj1X[i], y + adj1Y[i]));
                    }
                }
            }
        }

        foreach (var (fx, fy) in toFill)
        {
            var existing = area.GetTile(fx, fy);
            if (existing != null && !existing.Walkable)
            {
                var floor = Tile.DungeonFloor;
                floor.RegionId = _nextRegionId++;
                area.SetTile(fx, fy, floor);
                _floorTiles.Add((fx, fy));
            }
        }
    }

    private bool IsDoor(Tile tile) => tile.Name == "Open Door" || tile.Name == "Closed Door";

    private int CountCardinalWalkable(Area area, int x, int y)
    {
        int count = 0;
        if (area.GetTile(x - 1, y)?.Walkable == true) count++;
        if (area.GetTile(x + 1, y)?.Walkable == true) count++;
        if (area.GetTile(x, y - 1)?.Walkable == true) count++;
        if (area.GetTile(x, y + 1)?.Walkable == true) count++;
        return count;
    }

    #endregion

    #region Phase 7: Place Items

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

    #endregion

    #region Helper Classes

    private class BspNode
    {
        public int X, Y, Width, Height;
        public BspNode? Left, Right;
        public RoomInfo? Room;

        public bool IsLeaf => Left == null && Right == null;

        public BspNode(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    private class RoomInfo
    {
        public int Index;
        public int X, Y, Width, Height;
        public int RegionId;

        public int CenterX => X + Width / 2;
        public int CenterY => Y + Height / 2;

        public RoomInfo(int index, int x, int y, int width, int height, int regionId)
        {
            Index = index;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            RegionId = regionId;
        }
    }

    #endregion
}
