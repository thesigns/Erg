using Erg.Core.World.Critters;
using Erg.Core.World.Items;

namespace Erg.Core.World.Generators;

public class DungeonGenerator3 : IDungeonGenerator
{
    private readonly int _width;
    private readonly int _height;
    private readonly Random _random;
    private readonly int _level;

    private int _nextRegionId = 1;
    private readonly List<RoomInfo> _rooms = new();
    private readonly List<CorridorInfo> _corridors = new();
    private readonly HashSet<(int, int)> _floorTiles = new();
    private readonly HashSet<(int, int)> _caveTiles = new();
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

    // Cave generation
    private const int MinRemovedRoomsForCave = 3;  // Min removed rooms to generate cave

    public DungeonGenerator3(int width, int height, int seed, int level = 1)
    {
        _width = width;
        _height = height;
        _random = new Random(seed);
        _level = level;
    }

    public Area Generate()
    {
        var area = new Area(_width, _height, _level);

        // Fill with rock
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                area.SetTile(x, y, Tile.Rock);

        // Generate rooms
        GenerateRooms(area);

        // Connect rooms with corridors
        ConnectRooms(area);

        // Remove disconnected rooms
        int removedCount = RemoveDisconnectedRooms(area);

        // Generate caves in unused space (only if enough rooms were removed)
        if (removedCount >= MinRemovedRoomsForCave)
            GenerateCaves(area);

        // Specialize rooms
        SpecializeRooms(area);

        // Generate dead ends
        GenerateDeadEnds(area);

        // Process doors
        ProcessDoors(area);

        // Process walls
        ProcessWalls(area);

        // Process impenetrable rock on edges
        ProcessImpenetrableRock(area);

        // Place stairs
        PlaceStairs(area);

        // Place items in rooms
        PlaceItems(area);

        // Place critters
        PlaceCritters(area);

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
        var area = new Area(_width, _height, _level);

        // Fill with rock
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                area.SetTile(x, y, Tile.Rock);
        yield return new GenerationStep("Filled with rock", area);

        // Generate rooms with step-by-step feedback
        foreach (var step in GenerateRoomsStepByStep(area))
        {
            yield return step;
        }

        yield return new GenerationStep($"Phase 1 complete: {_rooms.Count} rooms", area);

        // Phase 2: Connect rooms with corridors
        foreach (var step in ConnectRoomsStepByStep(area))
        {
            yield return step;
        }

        // Phase 6: Process doors
        ProcessDoors(area);
        yield return new GenerationStep("Phase 6 complete: doors processed", area);

        // Phase 7: Process walls
        ProcessWalls(area);
        yield return new GenerationStep("Phase 7 complete: walls processed", area);

        // Phase 8: Process impenetrable rock on edges
        ProcessImpenetrableRock(area);
        yield return new GenerationStep("Phase 8 complete: map edges sealed", area);

        // Phase 9: Place stairs
        PlaceStairs(area);
        yield return new GenerationStep("Phase 9 complete: stairs placed", area);

        // Phase 10: Place items
        PlaceItems(area);
        yield return new GenerationStep("Phase 10 complete: items placed", area);

        // Phase 11: Place critters
        PlaceCritters(area);
        yield return new GenerationStep("Phase 11 complete: critters placed", area);
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
                    $"Room {result.Room!.Index}: ({result.Room.X},{result.Room.Y}) {result.Room.Width}x{result.Room.Height}",
                    area);
            }
            else
            {
                consecutiveFails++;
                // Don't yield on every fail - too spammy
                // Only report milestone fails
                if (consecutiveFails == 50)
                {
                    yield return new GenerationStep($"50 attempts failed...", area);
                }
            }
        }

        yield return new GenerationStep($"Finished after {MaxConsecutiveFails} failed attempts", area);
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
            yield return new GenerationStep("Too few rooms to connect", area);
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
                string targetType = result.TargetType == "room" ? $"room {result.TargetIndex}" : $"corridor {result.TargetIndex}";
                yield return new GenerationStep(
                    $"Corridor {_corridors.Count - 1}: room {result.SourceRoomIndex}→{targetType} ({start.x},{start.y})→({end.x},{end.y}) len.{result.Path.Count}",
                    area);
            }
            else
            {
                consecutiveFails++;
            }
        }

        int connectedRooms = _rooms.Count(r => r.Connected);
        int totalRooms = _rooms.Count;
        string status = connectedRooms == totalRooms ? "all connected" : $"{connectedRooms}/{totalRooms} rooms connected";
        yield return new GenerationStep($"Phase 2 complete: {_corridors.Count} corridors, {status}", area);

        // Phase 3: Remove disconnected rooms
        int removedCount = RemoveDisconnectedRooms(area);
        if (removedCount > 0)
        {
            yield return new GenerationStep($"Phase 3 complete: removed {removedCount} disconnected rooms", area);
        }
        else
        {
            yield return new GenerationStep("Phase 3 complete: no disconnected rooms", area);
        }

        // Phase 3b: Generate caves (only if enough rooms were removed)
        if (removedCount >= MinRemovedRoomsForCave)
        {
            GenerateCaves(area);
            if (_caveTiles.Count > 0)
            {
                var (cx, cy) = _caveTiles.First();
                yield return new GenerationStep($"Phase 3b complete: cave at ({cx},{cy})", area);
            }
            else
            {
                yield return new GenerationStep("Phase 3b complete: no space for cave", area);
            }
        }

        // Phase 4: Specialize rooms
        foreach (var step in SpecializeRoomsStepByStep(area))
        {
            yield return step;
        }

        // Phase 5: Generate dead ends
        foreach (var step in GenerateDeadEndsStepByStep(area))
        {
            yield return step;
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
            // Don't start a new network if main network exists but is maxed out
            if (_rooms.Any(r => r.Connected))
                return (false, -1, "", -1, null);

            // This branch now only executes when NO rooms are connected yet (first corridor)
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

    #region Phase 3b: Cave Generation

    private void GenerateCaves(Area area)
    {
        // Find the best spot for cave center using multi-source BFS from all floor tiles
        // Combined with distance from map edges

        // Step 1: Calculate distance from floors using multi-source BFS
        var distanceFromFloors = new int[_width, _height];
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                distanceFromFloors[x, y] = int.MaxValue;

        var queue = new Queue<(int x, int y)>();

        // Initialize BFS from all floor tiles AND map edges
        foreach (var (fx, fy) in _floorTiles)
        {
            distanceFromFloors[fx, fy] = 0;
            queue.Enqueue((fx, fy));
        }

        // Treat map edges as floor tiles (distance 0)
        for (int x = 0; x < _width; x++)
        {
            if (distanceFromFloors[x, 0] == int.MaxValue)
            {
                distanceFromFloors[x, 0] = 0;
                queue.Enqueue((x, 0));
            }
            if (distanceFromFloors[x, _height - 1] == int.MaxValue)
            {
                distanceFromFloors[x, _height - 1] = 0;
                queue.Enqueue((x, _height - 1));
            }
        }
        for (int y = 1; y < _height - 1; y++)
        {
            if (distanceFromFloors[0, y] == int.MaxValue)
            {
                distanceFromFloors[0, y] = 0;
                queue.Enqueue((0, y));
            }
            if (distanceFromFloors[_width - 1, y] == int.MaxValue)
            {
                distanceFromFloors[_width - 1, y] = 0;
                queue.Enqueue((_width - 1, y));
            }
        }

        // BFS to fill distance map (4-directional for Manhattan distance)
        var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            int currentDist = distanceFromFloors[x, y];

            foreach (var (dx, dy) in directions)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || nx >= _width || ny < 0 || ny >= _height)
                    continue;

                if (distanceFromFloors[nx, ny] > currentDist + 1)
                {
                    distanceFromFloors[nx, ny] = currentDist + 1;
                    queue.Enqueue((nx, ny));
                }
            }
        }

        // Step 2: Collect all rock tiles with maximum distance score
        var candidates = new List<(int x, int y)>();
        int bestScore = 0;

        // Margin of 2 from edges (for DungeonWall + ImpenetrableRock)
        for (int y = 2; y < _height - 2; y++)
        {
            for (int x = 2; x < _width - 2; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile == null || tile.Type != TileType.Rock)
                    continue;

                int score = distanceFromFloors[x, y];

                if (score > bestScore)
                {
                    bestScore = score;
                    candidates.Clear();
                    candidates.Add((x, y));
                }
                else if (score == bestScore)
                {
                    candidates.Add((x, y));
                }
            }
        }

        // Step 2b: Pick point closest to centroid of all candidates
        int bestX = -1, bestY = -1;
        if (candidates.Count > 0)
        {
            double centroidX = candidates.Average(c => c.x);
            double centroidY = candidates.Average(c => c.y);

            double minDist = double.MaxValue;
            foreach (var (cx, cy) in candidates)
            {
                double dist = (cx - centroidX) * (cx - centroidX) + (cy - centroidY) * (cy - centroidY);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestX = cx;
                    bestY = cy;
                }
            }
        }

        // Step 3: Generate cave from the center point
        if (bestX >= 0 && bestY >= 0)
        {
            GenerateCaveFromLocation(area, bestX, bestY);
        }
    }

    private void GenerateCaveFromLocation(Area area, int centerX, int centerY)
    {
        int caveRegionId = _nextRegionId++;
        var localCaveTiles = new HashSet<(int x, int y)>();

        // Phase 1: Drunkard's Walk - create main structure
        int numWalkers = _random.Next(4, 7);  // 4-6 walkers
        for (int i = 0; i < numWalkers; i++)
        {
            int steps = _random.Next(15, 31);  // 15-30 steps
            DrunkardWalk(area, centerX, centerY, localCaveTiles, caveRegionId, steps);
        }

        // Phase 2: Widen passages - make corridors thicker
        for (int pass = 0; pass < 2; pass++)
        {
            WidenCave(area, localCaveTiles, caveRegionId);
        }

        // Phase 3: Add stalagmites - rock columns in open areas
        AddStalagmites(area, localCaveTiles);

        // Phase 4: Connect to dungeon
        ConnectCaveToDungeon(area, localCaveTiles, caveRegionId);
    }

    private void DrunkardWalk(Area area, int startX, int startY, HashSet<(int x, int y)> caveTiles, int regionId, int maxSteps)
    {
        var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
        int x = startX;
        int y = startY;
        var dir = directions[_random.Next(4)];

        for (int step = 0; step < maxSteps; step++)
        {
            // Check bounds
            if (x < 2 || x >= _width - 2 || y < 2 || y >= _height - 2)
                break;

            // Check if we'd touch non-cave walkable
            if (TouchesNonCaveWalkable(area, x, y, caveTiles))
                break;

            // Place cave tile if it's rock
            var tile = area.GetTile(x, y);
            if (tile != null && tile.Type == TileType.Rock)
            {
                var caveTile = Tile.Floor(TileStructure.Cave);
                caveTile.RegionId = regionId;
                area.SetTile(x, y, caveTile);
                caveTiles.Add((x, y));
                _caveTiles.Add((x, y));
                _floorTiles.Add((x, y));
            }

            // 70% continue same direction, 30% change direction
            if (_random.Next(100) < 30)
            {
                dir = directions[_random.Next(4)];
            }

            x += dir.Item1;
            y += dir.Item2;
        }
    }

    private void WidenCave(Area area, HashSet<(int x, int y)> caveTiles, int regionId)
    {
        var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
        var tilesToAdd = new List<(int x, int y)>();

        foreach (var (cx, cy) in caveTiles)
        {
            foreach (var (dx, dy) in directions)
            {
                int nx = cx + dx;
                int ny = cy + dy;

                // Skip if out of bounds
                if (nx < 2 || nx >= _width - 2 || ny < 2 || ny >= _height - 2)
                    continue;

                // Skip if already cave
                if (caveTiles.Contains((nx, ny)))
                    continue;

                var tile = area.GetTile(nx, ny);
                if (tile == null || tile.Type != TileType.Rock)
                    continue;

                // Skip if touches non-cave walkable
                if (TouchesNonCaveWalkable(area, nx, ny, caveTiles))
                    continue;

                // 40% chance to widen
                if (_random.Next(100) < 40)
                {
                    tilesToAdd.Add((nx, ny));
                }
            }
        }

        foreach (var (x, y) in tilesToAdd)
        {
            if (caveTiles.Contains((x, y))) continue;  // May have been added by another iteration

            var caveTile = Tile.Floor(TileStructure.Cave);
            caveTile.RegionId = regionId;
            area.SetTile(x, y, caveTile);
            caveTiles.Add((x, y));
            _caveTiles.Add((x, y));
            _floorTiles.Add((x, y));
        }
    }

    private void AddStalagmites(Area area, HashSet<(int x, int y)> caveTiles)
    {
        var tilesToConvert = new List<(int x, int y)>();

        foreach (var (cx, cy) in caveTiles)
        {
            // Count cave neighbors in 3x3 area
            int caveNeighbors = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (caveTiles.Contains((cx + dx, cy + dy)))
                        caveNeighbors++;
                }
            }

            // If surrounded by cave (>=7 neighbors), 8% chance for stalagmite
            if (caveNeighbors >= 7 && _random.Next(100) < 8)
            {
                tilesToConvert.Add((cx, cy));
            }
        }

        foreach (var (x, y) in tilesToConvert)
        {
            area.SetTile(x, y, Tile.Rock);
            caveTiles.Remove((x, y));
            _caveTiles.Remove((x, y));
            _floorTiles.Remove((x, y));
        }
    }

    private void ConnectCaveToDungeon(Area area, HashSet<(int x, int y)> caveTiles, int regionId)
    {
        if (caveTiles.Count == 0) return;

        // Find the cave tile closest to any dungeon floor
        int bestCaveX = -1, bestCaveY = -1;
        int bestDungeonX = -1, bestDungeonY = -1;
        int bestDistance = int.MaxValue;

        foreach (var (cx, cy) in caveTiles)
        {
            foreach (var (fx, fy) in _floorTiles)
            {
                // Skip cave tiles
                if (caveTiles.Contains((fx, fy))) continue;

                int dist = Math.Abs(cx - fx) + Math.Abs(cy - fy);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestCaveX = cx;
                    bestCaveY = cy;
                    bestDungeonX = fx;
                    bestDungeonY = fy;
                }
            }
        }

        if (bestCaveX < 0) return;

        // Dig a corridor from cave to dungeon
        int x = bestCaveX;
        int y = bestCaveY;

        // First move horizontally, then vertically
        int dx = bestDungeonX > x ? 1 : (bestDungeonX < x ? -1 : 0);
        int dy = bestDungeonY > y ? 1 : (bestDungeonY < y ? -1 : 0);

        // Horizontal part
        while (x != bestDungeonX)
        {
            x += dx;
            var tile = area.GetTile(x, y);
            if (tile != null && !tile.Walkable)
            {
                var corridorTile = Tile.Floor(TileStructure.Cave);
                corridorTile.RegionId = regionId;
                area.SetTile(x, y, corridorTile);
                caveTiles.Add((x, y));
                _caveTiles.Add((x, y));
                _floorTiles.Add((x, y));
            }
            else if (tile != null && tile.Walkable && tile.Structure != TileStructure.Cave)
            {
                // Reached dungeon - place door one tile back
                x -= dx;
                if (!caveTiles.Contains((x, y)))
                {
                    var door = Tile.ClosedDoor;
                    door.RegionId = regionId;
                    area.SetTile(x, y, door);
                }
                return;
            }
        }

        // Vertical part
        while (y != bestDungeonY)
        {
            y += dy;
            var tile = area.GetTile(x, y);
            if (tile != null && !tile.Walkable)
            {
                var corridorTile = Tile.Floor(TileStructure.Cave);
                corridorTile.RegionId = regionId;
                area.SetTile(x, y, corridorTile);
                caveTiles.Add((x, y));
                _caveTiles.Add((x, y));
                _floorTiles.Add((x, y));
            }
            else if (tile != null && tile.Walkable && tile.Structure != TileStructure.Cave)
            {
                // Reached dungeon - place door one tile back
                y -= dy;
                if (!caveTiles.Contains((x, y)))
                {
                    var door = Tile.ClosedDoor;
                    door.RegionId = regionId;
                    area.SetTile(x, y, door);
                }
                return;
            }
        }
    }

    private bool TouchesNonCaveWalkable(Area area, int x, int y, HashSet<(int x, int y)> caveTiles)
    {
        // Check all 8 directions for walkable non-cave tiles
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || nx >= _width || ny < 0 || ny >= _height)
                    continue;

                // Skip if it's already a cave tile
                if (caveTiles.Contains((nx, ny)))
                    continue;

                var tile = area.GetTile(nx, ny);
                if (tile != null && tile.Walkable)
                    return true;  // Found walkable non-cave tile
            }
        }
        return false;
    }

    #endregion

    #region Phase 4: Room Specialization

    private enum RoomShapeSpecialization
    {
        None,
        CornerColumns,
        RoundedCorners,
        CenterCross,
        CenterCrossRoundedCorners
    }

    private enum RoomFeatureSpecialization
    {
        None,
        WaterContainer,
        Graveyard
    }

    private void SpecializeRooms(Area area)
    {
        SpecializeRoomShapes(area);
        SpecializeRoomFeatures(area);
    }

    private void SpecializeRoomShapes(Area area)
    {
        foreach (var room in _rooms)
        {
            // 50% chance for shape specialization
            if (_random.Next(100) >= 50)
                continue;

            var specialization = RollShapeSpecialization();
            ApplyShapeSpecialization(area, room, specialization);
        }
    }

    private void SpecializeRoomFeatures(Area area)
    {
        foreach (var room in _rooms)
        {
            // 20% chance for feature specialization
            if (_random.Next(100) >= 20)
                continue;

            var specialization = RollFeatureSpecialization();
            ApplyFeatureSpecialization(area, room, specialization);
        }
    }

    private IEnumerable<GenerationStep> SpecializeRoomsStepByStep(Area area)
    {
        int shapeCount = 0;
        int featureCount = 0;

        // Phase 4a: Shape specialization
        foreach (var room in _rooms)
        {
            if (_random.Next(100) >= 50)
                continue;

            var specialization = RollShapeSpecialization();
            ApplyShapeSpecialization(area, room, specialization);
            shapeCount++;

            yield return new GenerationStep(
                $"Room {room.Index}: shape {specialization}",
                area);
        }

        yield return new GenerationStep(
            $"Phase 4a complete: {shapeCount} rooms with special shape",
            area);

        // Phase 4b: Feature specialization
        foreach (var room in _rooms)
        {
            if (_random.Next(100) >= 20)
                continue;

            var specialization = RollFeatureSpecialization();
            ApplyFeatureSpecialization(area, room, specialization);
            featureCount++;

            yield return new GenerationStep(
                $"Room {room.Index}: feature {specialization}",
                area);
        }

        yield return new GenerationStep(
            $"Phase 4b complete: {featureCount} rooms with special feature",
            area);
    }

    private RoomShapeSpecialization RollShapeSpecialization()
    {
        int roll = _random.Next(100);

        if (roll < 25)
            return RoomShapeSpecialization.CornerColumns;
        else if (roll < 50)
            return RoomShapeSpecialization.RoundedCorners;
        else if (roll < 75)
            return RoomShapeSpecialization.CenterCross;
        else
            return RoomShapeSpecialization.CenterCrossRoundedCorners;
    }

    private RoomFeatureSpecialization RollFeatureSpecialization()
    {
        int roll = _random.Next(100);
        if (roll < 50)
            return RoomFeatureSpecialization.WaterContainer;
        else
            return RoomFeatureSpecialization.Graveyard;
    }

    private void ApplyShapeSpecialization(Area area, RoomInfo room, RoomShapeSpecialization specialization)
    {
        switch (specialization)
        {
            case RoomShapeSpecialization.CornerColumns:
                ApplyCornerColumns(area, room);
                break;
            case RoomShapeSpecialization.RoundedCorners:
                ApplyRoundedCorners(area, room);
                break;
            case RoomShapeSpecialization.CenterCross:
                ApplyCenterCross(area, room);
                break;
            case RoomShapeSpecialization.CenterCrossRoundedCorners:
                ApplyCenterCrossRoundedCorners(area, room);
                break;
        }
    }

    private void ApplyFeatureSpecialization(Area area, RoomInfo room, RoomFeatureSpecialization specialization)
    {
        switch (specialization)
        {
            case RoomFeatureSpecialization.WaterContainer:
                ApplyWaterContainer(area, room);
                break;
            case RoomFeatureSpecialization.Graveyard:
                ApplyGraveyard(area, room);
                break;
        }
    }

    private void ApplyCornerColumns(Area area, RoomInfo room)
    {
        // Place rock columns at inner corners (1 tile from walls)
        var columnPositions = new[]
        {
            (room.X + 1, room.Y + 1),                         // Top-left
            (room.X + room.Width - 2, room.Y + 1),            // Top-right
            (room.X + 1, room.Y + room.Height - 2),           // Bottom-left
            (room.X + room.Width - 2, room.Y + room.Height - 2) // Bottom-right
        };

        foreach (var (x, y) in columnPositions)
        {
            area.SetTile(x, y, Tile.Rock);
            _floorTiles.Remove((x, y));
        }
    }

    private void ApplyRoundedCorners(Area area, RoomInfo room)
    {
        // Actual corner positions of the room
        var cornerPositions = new[]
        {
            (room.X, room.Y),                                 // Top-left
            (room.X + room.Width - 1, room.Y),                // Top-right
            (room.X, room.Y + room.Height - 1),               // Bottom-left
            (room.X + room.Width - 1, room.Y + room.Height - 1) // Bottom-right
        };

        foreach (var (x, y) in cornerPositions)
        {
            // Check if corner is adjacent to an entrance (horizontally or vertically)
            if (IsAdjacentToEntrance(area, x, y))
                continue;

            // Not adjacent to entrance - replace with rock
            area.SetTile(x, y, Tile.Rock);
            _floorTiles.Remove((x, y));
        }
    }

    private bool IsAdjacentToEntrance(Area area, int x, int y)
    {
        var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

        foreach (var (dx, dy) in directions)
        {
            int nx = x + dx;
            int ny = y + dy;

            if (nx < 0 || nx >= _width || ny < 0 || ny >= _height)
                continue;

            var tile = area.GetTile(nx, ny);
            if (tile != null && tile.Structure == TileStructure.Entrance)
                return true;
        }

        return false;
    }

    private void ApplyWaterContainer(Area area, RoomInfo room)
    {
        // Pass 1: Fill all walkable tiles with DeepWater
        for (int y = room.Y; y < room.Y + room.Height; y++)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
            {
                var currentTile = area.GetTile(x, y);
                if (currentTile != null && currentTile.Walkable)
                {
                    var waterTile = Tile.DeepWater;
                    waterTile.RegionId = room.RegionId;
                    area.SetTile(x, y, waterTile);
                    _floorTiles.Remove((x, y));
                }
            }
        }

        // Pass 2: Convert DeepWater adjacent to non-walkable tiles to ShallowWater
        for (int y = room.Y; y < room.Y + room.Height; y++)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
            {
                var currentTile = area.GetTile(x, y);
                if (currentTile != null && currentTile.Type == TileType.DeepWater && IsAdjacentToSolidTile(area, x, y))
                {
                    var shallowTile = Tile.ShallowWater;
                    shallowTile.RegionId = room.RegionId;
                    area.SetTile(x, y, shallowTile);
                }
            }
        }
    }

    private bool IsAdjacentToSolidTile(Area area, int x, int y)
    {
        // Check all 8 directions (including diagonals)
        // Returns true if adjacent to a solid tile (wall/rock), excluding water tiles
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || nx >= _width || ny < 0 || ny >= _height)
                    continue;

                var neighbor = area.GetTile(nx, ny);
                if (neighbor != null && !neighbor.Walkable &&
                    neighbor.Type != TileType.DeepWater &&
                    neighbor.Type != TileType.ShallowWater)
                    return true;
            }
        }
        return false;
    }

    private void ApplyGraveyard(Area area, RoomInfo room)
    {
        // Set UndeadAura on ALL tiles and replace 20% of Floor tiles with Grave
        for (int y = room.Y; y < room.Y + room.Height; y++)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
            {
                var currentTile = area.GetTile(x, y);
                if (currentTile == null) continue;

                // Set UndeadAura on all tiles in the room
                currentTile.SpecialEffect = SpecialEffect.UndeadAura;

                // 20% chance to replace Floor with Grave
                if (currentTile.Type == TileType.Floor && _random.Next(100) < 20)
                {
                    var grave = Tile.Grave(_random);
                    grave.RegionId = room.RegionId;
                    grave.SpecialEffect = SpecialEffect.UndeadAura;
                    area.SetTile(x, y, grave);
                }
            }
        }
    }

    private void ApplyCenterCross(Area area, RoomInfo room)
    {
        // Requires minimum 5x5 room
        if (room.Width < 5 || room.Height < 5)
            return;

        var crossTiles = new HashSet<(int, int)>();

        // Determine center column(s)
        int cx1, cx2;
        if (room.Width % 2 == 1) // odd width - single center column
        {
            cx1 = cx2 = room.X + room.Width / 2;
        }
        else // even width - 2 center columns
        {
            cx1 = room.X + room.Width / 2 - 1;
            cx2 = room.X + room.Width / 2;
        }

        // Determine center row(s)
        int cy1, cy2;
        if (room.Height % 2 == 1) // odd height - single center row
        {
            cy1 = cy2 = room.Y + room.Height / 2;
        }
        else // even height - 2 center rows
        {
            cy1 = room.Y + room.Height / 2 - 1;
            cy2 = room.Y + room.Height / 2;
        }

        // Vertical arm: center columns, extending 1 row above and below center
        for (int x = cx1; x <= cx2; x++)
        {
            for (int y = cy1 - 1; y <= cy2 + 1; y++)
            {
                crossTiles.Add((x, y));
            }
        }

        // Horizontal arm: center rows, extending 1 column left and right of center
        for (int y = cy1; y <= cy2; y++)
        {
            for (int x = cx1 - 1; x <= cx2 + 1; x++)
            {
                crossTiles.Add((x, y));
            }
        }

        // Apply tiles
        foreach (var (x, y) in crossTiles)
        {
            area.SetTile(x, y, Tile.Rock);
            _floorTiles.Remove((x, y));
        }
    }

    private void ApplyCenterCrossRoundedCorners(Area area, RoomInfo room)
    {
        // Requires minimum 5x5 room (from CenterCross requirement)
        if (room.Width < 5 || room.Height < 5)
            return;

        // Apply both specializations
        ApplyCenterCross(area, room);
        ApplyRoundedCorners(area, room);
    }

    #endregion

    #region Phase 5: Dead End Generation

    private void GenerateDeadEnds(Area area)
    {
        int deadEndsCreated = 0;
        int maxAttempts = 300;
        int attempts = 0;

        while (deadEndsCreated < 20 && attempts < maxAttempts)
        {
            attempts++;
            if (TryCreateDeadEnd(area))
                deadEndsCreated++;
        }
    }

    private IEnumerable<GenerationStep> GenerateDeadEndsStepByStep(Area area)
    {
        int deadEndsCreated = 0;
        int maxAttempts = 100;
        int attempts = 0;

        while (deadEndsCreated < 10 && attempts < maxAttempts)
        {
            attempts++;
            if (TryCreateDeadEnd(area))
            {
                deadEndsCreated++;
                yield return new GenerationStep($"Dead end {deadEndsCreated} created", area);
            }
        }

        yield return new GenerationStep($"Phase 5 complete: {deadEndsCreated} dead ends", area);
    }

    private bool TryCreateDeadEnd(Area area)
    {
        // Find candidate rocks: adjacent to exactly one corridor floor
        var candidates = new List<(int x, int y, int dx, int dy)>();

        for (int scanY = 1; scanY < _height - 1; scanY++)
        {
            for (int scanX = 1; scanX < _width - 1; scanX++)
            {
                var tile = area.GetTile(scanX, scanY);
                if (tile == null || tile.Type != TileType.Rock)
                    continue;

                // Check if adjacent to exactly one corridor floor
                var corridorNeighbor = GetSingleCorridorNeighbor(area, scanX, scanY);
                if (corridorNeighbor != null)
                {
                    // Direction is opposite to the corridor (going away from it)
                    int dirX = scanX - corridorNeighbor.Value.x;
                    int dirY = scanY - corridorNeighbor.Value.y;
                    candidates.Add((scanX, scanY, dirX, dirY));
                }
            }
        }

        if (candidates.Count == 0)
            return false;

        // Pick random candidate
        var candidate = candidates[_random.Next(candidates.Count)];

        // Get the source corridor position to exclude from adjacency checks
        var sourceCorridorPos = GetSingleCorridorNeighbor(area, candidate.x, candidate.y)!.Value;

        // Carve the dead end
        var carvedTiles = new List<(int x, int y)>();
        int x = candidate.x;
        int y = candidate.y;
        int dx = candidate.dx;
        int dy = candidate.dy;
        int segmentLength = _random.Next(3, 7);
        int stepsInSegment = 0;

        while (true)
        {
            // Check if we can carve this tile
            if (!CanCarveDeadEndTile(area, x, y, carvedTiles, sourceCorridorPos))
                break;

            // Carve the tile
            var corridorTile = Tile.Floor(TileStructure.Corridor);
            corridorTile.RegionId = _nextRegionId;
            area.SetTile(x, y, corridorTile);
            carvedTiles.Add((x, y));
            _floorTiles.Add((x, y));

            stepsInSegment++;

            // Check if we should change direction
            if (stepsInSegment >= segmentLength)
            {
                // Try to turn 90 degrees
                var newDir = TryTurn90Degrees(area, x, y, dx, dy, carvedTiles, sourceCorridorPos);
                if (newDir == null)
                    break; // Can't turn, end here

                dx = newDir.Value.dx;
                dy = newDir.Value.dy;
                segmentLength = _random.Next(3, 7);
                stepsInSegment = 0;
            }

            // Move to next position
            x += dx;
            y += dy;

            // Safety limit
            if (carvedTiles.Count > 30)
                break;
        }

        if (carvedTiles.Count > 0)
        {
            _nextRegionId++;
            return true;
        }

        return false;
    }

    private (int x, int y)? GetSingleCorridorNeighbor(Area area, int x, int y)
    {
        var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
        (int x, int y)? found = null;
        int count = 0;

        foreach (var (dx, dy) in directions)
        {
            int nx = x + dx;
            int ny = y + dy;

            var tile = area.GetTile(nx, ny);
            if (tile != null && tile.Structure == TileStructure.Corridor)
            {
                count++;
                found = (nx, ny);
            }
        }

        return count == 1 ? found : null;
    }

    private bool CanCarveDeadEndTile(Area area, int x, int y, List<(int x, int y)> carvedTiles, (int x, int y) sourceCorridorPos)
    {
        // Must be within bounds (with margin of 2 for DungeonWall + ImpenetrableRock on edges)
        if (x < 2 || x >= _width - 2 || y < 2 || y >= _height - 2)
            return false;

        // Must be rock
        var tile = area.GetTile(x, y);
        if (tile == null || tile.Type != TileType.Rock)
            return false;

        // Check if carving this would break through to another area
        // Count adjacent walkable tiles (including diagonals), excluding our carved tiles and source corridor
        int adjacentWalkable = CountAdjacentWalkable(area, x, y, carvedTiles, sourceCorridorPos);

        // If any adjacent walkable tile (not counting our carved corridor and source), we'd break through
        return adjacentWalkable == 0;
    }

    private int CountAdjacentWalkable(Area area, int x, int y, List<(int x, int y)> excludeTiles, (int x, int y) sourceCorridorPos)
    {
        var allDirections = new[]
        {
            (-1, -1), (0, -1), (1, -1),
            (-1, 0),          (1, 0),
            (-1, 1),  (0, 1),  (1, 1)
        };

        int count = 0;
        foreach (var (dx, dy) in allDirections)
        {
            int nx = x + dx;
            int ny = y + dy;

            // Skip our own carved tiles
            if (excludeTiles.Contains((nx, ny)))
                continue;

            // Skip the source corridor
            if (nx == sourceCorridorPos.x && ny == sourceCorridorPos.y)
                continue;

            var tile = area.GetTile(nx, ny);
            if (tile != null && tile.Walkable)
                count++;
        }

        return count;
    }

    private (int dx, int dy)? TryTurn90Degrees(Area area, int x, int y, int currentDx, int currentDy, List<(int x, int y)> carvedTiles, (int x, int y) sourceCorridorPos)
    {
        // Get perpendicular directions
        var turns = new List<(int dx, int dy)>();

        if (currentDx != 0) // Moving horizontally, try vertical
        {
            turns.Add((0, -1));
            turns.Add((0, 1));
        }
        else // Moving vertically, try horizontal
        {
            turns.Add((-1, 0));
            turns.Add((1, 0));
        }

        // Shuffle
        if (_random.Next(2) == 0)
            turns.Reverse();

        // Try each direction
        foreach (var (dx, dy) in turns)
        {
            int nx = x + dx;
            int ny = y + dy;

            if (CanCarveDeadEndTile(area, nx, ny, carvedTiles, sourceCorridorPos))
                return (dx, dy);
        }

        return null;
    }

    #endregion

    #region Phase 6: Door Processing

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

    #region Phase 7: Wall Processing

    private void ProcessWalls(Area area)
    {
        // Przeleć przez wszystkie tilesy z wyłączeniem krawędzi mapy
        for (int y = 1; y < _height - 1; y++)
        {
            for (int x = 1; x < _width - 1; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile == null || tile.Type != TileType.Rock)
                    continue;

                // Sprawdź czy sąsiaduje z nie-skałą (8 kierunków)
                if (HasNonRockNeighbor(area, x, y))
                {
                    // Nie zamieniaj na DungeonWall jeśli sąsiaduje z Cave (naturalne skały)
                    if (HasCaveNeighbor(area, x, y))
                        continue;

                    area.SetTile(x, y, Tile.DungeonWall);
                }
            }
        }
    }

    private bool HasCaveNeighbor(Area area, int x, int y)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || nx >= _width || ny < 0 || ny >= _height)
                    continue;

                var tile = area.GetTile(nx, ny);
                if (tile != null && tile.Structure == TileStructure.Cave)
                    return true;
            }
        }
        return false;
    }

    private bool HasNonRockNeighbor(Area area, int x, int y)
    {
        // 8 kierunków: góra, dół, lewo, prawo + 4 skosy
        var directions = new[]
        {
            (-1, -1), (0, -1), (1, -1),
            (-1,  0),          (1,  0),
            (-1,  1), (0,  1), (1,  1)
        };

        foreach (var (dx, dy) in directions)
        {
            int nx = x + dx;
            int ny = y + dy;

            // Sprawdź granice mapy
            if (nx < 0 || nx >= _width || ny < 0 || ny >= _height)
                continue;

            var neighbor = area.GetTile(nx, ny);
            if (neighbor != null && neighbor.Type != TileType.Rock && neighbor.Type != TileType.Wall)
                return true;
        }

        return false;
    }

    #endregion

    #region Phase 8: Impenetrable Rock Processing

    private void ProcessImpenetrableRock(Area area)
    {
        // Górna i dolna krawędź
        for (int x = 0; x < _width; x++)
        {
            area.SetTile(x, 0, Tile.ImpenetrableRock);
            area.SetTile(x, _height - 1, Tile.ImpenetrableRock);
        }

        // Lewa i prawa krawędź (bez rogów, już ustawione)
        for (int y = 1; y < _height - 1; y++)
        {
            area.SetTile(0, y, Tile.ImpenetrableRock);
            area.SetTile(_width - 1, y, Tile.ImpenetrableRock);
        }
    }

    #endregion

    #region Phase 9: Stairs Placement

    private void PlaceStairs(Area area)
    {
        // Zbierz wszystkie floor tilesy
        var roomFloors = new List<(int x, int y)>();
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile != null && tile.Type == TileType.Floor)
                {
                    roomFloors.Add((x, y));
                }
            }
        }

        if (roomFloors.Count < 2) return;

        // Umieść StairsUp na losowym ufloorze
        int upIndex = _random.Next(roomFloors.Count);
        var (upX, upY) = roomFloors[upIndex];
        var stairsUp = Tile.StairsUp;
        stairsUp.RegionId = area.GetTile(upX, upY)!.RegionId;
        area.SetTile(upX, upY, stairsUp);
        roomFloors.RemoveAt(upIndex);

        // Ustaw pozycję startową gracza na StairsUp
        _playerStart = (upX, upY);

        // Wylosuj do 3 kandydatów na StairsDown
        var candidates = new List<(int x, int y)>();
        int candidateCount = Math.Min(3, roomFloors.Count);
        for (int i = 0; i < candidateCount; i++)
        {
            int idx = _random.Next(roomFloors.Count);
            candidates.Add(roomFloors[idx]);
            roomFloors.RemoveAt(idx);
        }

        // Wybierz kandydata najdalszego od StairsUp
        (int x, int y) farthest = candidates[0];
        double maxDistance = 0;
        foreach (var (cx, cy) in candidates)
        {
            double distance = Math.Sqrt((cx - upX) * (cx - upX) + (cy - upY) * (cy - upY));
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthest = (cx, cy);
            }
        }

        // Umieść StairsDown
        var stairsDown = Tile.StairsDown;
        stairsDown.RegionId = area.GetTile(farthest.x, farthest.y)!.RegionId;
        area.SetTile(farthest.x, farthest.y, stairsDown);
    }

    #endregion

    #region Phase 10: Item Placement

    private void PlaceItems(Area area)
    {
        // Collect all valid floor tiles (Room, Corridor, or Cave)
        var validFloors = new List<(int x, int y)>();
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile != null && tile.Type == TileType.Floor &&
                    (tile.Structure == TileStructure.Room ||
                     tile.Structure == TileStructure.Corridor ||
                     tile.Structure == TileStructure.Cave))
                {
                    validFloors.Add((x, y));
                }
            }
        }

        if (validFloors.Count == 0) return;

        // Place 2-10 items
        int itemCount = _random.Next(2, 11);
        for (int i = 0; i < itemCount && validFloors.Count > 0; i++)
        {
            // Pick random floor tile
            int index = _random.Next(validFloors.Count);
            var (x, y) = validFloors[index];
            validFloors.RemoveAt(index);  // Don't place multiple items on same tile

            // Create Gold Coin with random count 1-100
            int goldAmount = _random.Next(1, 101);
            var coin = new GoldCoin(x, y, goldAmount);
            area.AddItem(coin);
        }
    }

    #endregion

    #region Phase 11: Critter Placement

    private void PlaceCritters(Area area)
    {
        var availableFloors = _floorTiles.ToList();

        if (availableFloors.Count == 0) return;

        int critterCount = _random.Next(4, 9); // 4-8 przeciwników
        for (int i = 0; i < critterCount && availableFloors.Count > 0; i++)
        {
            int index = _random.Next(availableFloors.Count);
            var (x, y) = availableFloors[index];
            availableFloors.RemoveAt(index);

            // Skip if tile already has a critter
            var tile = area.GetTile(x, y);
            if (tile?.Critter != null) continue;

            // 50% Dummy, 50% SpinningDummy
            Critter critter = _random.Next(2) == 0
                ? new Dummy(x, y)
                : new SpinningDummy(x, y);

            area.SetCritter(critter);
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
