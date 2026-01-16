namespace Erg.Core.World.Pathfinding;

public static class Pathfinder
{
    private static readonly (int dx, int dy)[] Directions =
    {
        (-1, -1), (0, -1), (1, -1),
        (-1,  0),          (1,  0),
        (-1,  1), (0,  1), (1,  1)
    };

    private const float DiagonalCost = 1.414f;
    private const float StraightCost = 1.0f;

    /// <summary>
    /// Finds a path from critter's current position to (goalX, goalY).
    /// Returns list of (x, y) steps or empty list if no path found.
    /// First element is the next step to take.
    /// </summary>
    public static List<(int x, int y)> FindPath(
        Area area,
        Critter critter,
        int goalX,
        int goalY,
        bool avoidCritters = true,
        int maxNodes = 200)
    {
        int startX = critter.X;
        int startY = critter.Y;

        // Already at goal
        if (startX == goalX && startY == goalY)
            return new List<(int, int)>();

        // Open set: nodes to evaluate (priority queue by F cost)
        var openSet = new PriorityQueue<(int x, int y), float>();

        // G costs: actual cost from start to node
        var gCosts = new Dictionary<(int, int), float>();

        // Parents: for path reconstruction
        var parents = new Dictionary<(int, int), (int x, int y)>();

        // Closed set: already evaluated
        var closedSet = new HashSet<(int, int)>();

        // Initialize start node
        var start = (startX, startY);
        gCosts[start] = 0;
        openSet.Enqueue(start, Heuristic(startX, startY, goalX, goalY));

        int nodesProcessed = 0;

        while (openSet.Count > 0 && nodesProcessed < maxNodes)
        {
            var current = openSet.Dequeue();
            nodesProcessed++;

            // Skip if already processed (can happen with priority queue)
            if (closedSet.Contains(current))
                continue;

            closedSet.Add(current);

            // Goal reached
            if (current.x == goalX && current.y == goalY)
                return ReconstructPath(parents, current);

            // Explore neighbors
            foreach (var (dx, dy) in Directions)
            {
                int nx = current.x + dx;
                int ny = current.y + dy;
                var neighbor = (nx, ny);

                // Skip if already evaluated
                if (closedSet.Contains(neighbor))
                    continue;

                // Check if tile exists
                var tile = area.GetTile(nx, ny);
                if (tile == null)
                    continue;

                // Check if critter can enter this tile
                if (!critter.CanEnterTile(tile))
                    continue;

                // Check for blocking critters (unless it's the goal - we want to path TO enemies)
                if (avoidCritters && !(nx == goalX && ny == goalY))
                {
                    if (area.GetBlockingCritter(nx, ny) != null)
                        continue;
                }

                // Calculate movement cost
                bool isDiagonal = dx != 0 && dy != 0;
                float baseCost = isDiagonal ? DiagonalCost : StraightCost;
                float terrainMultiplier = critter.GetMovementCostMultiplier(tile);
                float moveCost = baseCost * terrainMultiplier;

                float tentativeG = gCosts[current] + moveCost;

                // Skip if we already have a better path to this node
                if (gCosts.TryGetValue(neighbor, out float existingG) && tentativeG >= existingG)
                    continue;

                // This is a better path
                gCosts[neighbor] = tentativeG;
                parents[neighbor] = current;

                float h = Heuristic(nx, ny, goalX, goalY);
                float f = tentativeG + h;
                openSet.Enqueue(neighbor, f);
            }
        }

        // No path found
        return new List<(int, int)>();
    }

    /// <summary>
    /// Chebyshev distance - appropriate for 8-directional movement.
    /// </summary>
    private static float Heuristic(int x1, int y1, int x2, int y2)
    {
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        return Math.Max(dx, dy);
    }

    /// <summary>
    /// Reconstructs path from parents dictionary.
    /// Returns path from first step to goal (excludes start position).
    /// </summary>
    private static List<(int x, int y)> ReconstructPath(
        Dictionary<(int, int), (int x, int y)> parents,
        (int x, int y) goal)
    {
        var path = new List<(int x, int y)>();
        var current = goal;

        while (parents.ContainsKey(current))
        {
            path.Add(current);
            current = parents[current];
        }

        path.Reverse();
        return path;
    }
}
