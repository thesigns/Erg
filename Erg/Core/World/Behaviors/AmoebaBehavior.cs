namespace Erg.Core.World.Behaviors;

public class AmoebaBehavior : IBehavior
{
    public static readonly AmoebaBehavior Instance = new();

    private static readonly (int dx, int dy)[] Directions =
    {
        (-1, -1), (0, -1), (1, -1),
        (-1,  0),          (1,  0),
        (-1,  1), (0,  1), (1,  1)
    };

    public CritterAction DecideAction(Critter critter, Session session)
    {
        // 1. Scan radius 2 for critters, update enemies
        UpdateEnemies(critter, session);

        var enemy = critter.CurrentEnemy;

        // 2. If adjacent to enemy, attack
        if (enemy != null && IsAdjacent(critter, enemy))
            return new MeleeAttackAction(enemy);

        // 3. If enemy exists, move toward it (even onto land)
        if (enemy != null)
        {
            var moveToward = GetMoveToward(critter, enemy, session);
            if (moveToward != null)
                return moveToward;
        }

        // 4. No enemy and on land - return to water
        var currentTile = session.Area.GetTile(critter.X, critter.Y);
        if (currentTile != null && !currentTile.Swimmable)
        {
            var waterMove = FindAdjacentWater(critter, session);
            if (waterMove != null)
                return waterMove;
        }

        // 5. Random movement in water
        var randomMove = GetRandomWaterMove(critter, session);
        if (randomMove != null)
            return randomMove;

        return WaitAction.Instance;
    }

    private MoveAction? FindAdjacentWater(Critter critter, Session session)
    {
        var waterTiles = new List<(int dx, int dy)>();

        foreach (var (dx, dy) in Directions)
        {
            var tile = session.Area.GetTile(critter.X + dx, critter.Y + dy);
            if (tile != null && tile.Swimmable && critter.CanEnterTile(tile))
            {
                if (session.Area.GetBlockingCritter(critter.X + dx, critter.Y + dy) == null)
                    waterTiles.Add((dx, dy));
            }
        }

        if (waterTiles.Count == 0)
            return null;

        var (rdx, rdy) = waterTiles[session.Random.Next(waterTiles.Count)];
        return new MoveAction(rdx, rdy);
    }

    private void UpdateEnemies(Critter critter, Session session)
    {
        // Clear all enemies (amoeba forgets quickly)
        foreach (var oldEnemy in critter.Enemies.ToList())
            critter.RemoveEnemy(oldEnemy);

        // Scan radius 2
        Critter? nearest = null;
        int nearestDist = int.MaxValue;

        for (int dy = -2; dy <= 2; dy++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                var other = session.Area.GetCritter(critter.X + dx, critter.Y + dy);
                if (other != null && other != critter)
                {
                    int dist = Math.Max(Math.Abs(dx), Math.Abs(dy)); // Chebyshev distance
                    if (dist < nearestDist)
                    {
                        nearest = other;
                        nearestDist = dist;
                    }
                }
            }
        }

        if (nearest != null)
            critter.AddEnemy(nearest);
    }

    private static bool IsAdjacent(Critter a, Critter b)
    {
        int dx = Math.Abs(a.X - b.X);
        int dy = Math.Abs(a.Y - b.Y);
        return dx <= 1 && dy <= 1 && (dx + dy > 0);
    }

    private MoveAction? GetMoveToward(Critter critter, Critter target, Session session)
    {
        int dx = Math.Sign(target.X - critter.X);
        int dy = Math.Sign(target.Y - critter.Y);

        // Try direct path first
        if (CanMoveTo(critter, dx, dy, session))
            return new MoveAction(dx, dy);

        // Try horizontal only
        if (dx != 0 && CanMoveTo(critter, dx, 0, session))
            return new MoveAction(dx, 0);

        // Try vertical only
        if (dy != 0 && CanMoveTo(critter, 0, dy, session))
            return new MoveAction(0, dy);

        return null;
    }

    private MoveAction? GetRandomWaterMove(Critter critter, Session session)
    {
        var currentTile = session.Area.GetTile(critter.X, critter.Y);
        bool inWater = currentTile?.Swimmable ?? false;

        var validMoves = new List<(int dx, int dy)>();

        foreach (var (dx, dy) in Directions)
        {
            var tile = session.Area.GetTile(critter.X + dx, critter.Y + dy);
            if (tile == null || !critter.CanEnterTile(tile))
                continue;
            if (session.Area.GetBlockingCritter(critter.X + dx, critter.Y + dy) != null)
                continue;

            // If in water, only consider water tiles (don't leave without reason)
            if (inWater && !tile.Swimmable)
                continue;

            validMoves.Add((dx, dy));
        }

        if (validMoves.Count == 0)
            return null;

        var (rdx, rdy) = validMoves[session.Random.Next(validMoves.Count)];
        return new MoveAction(rdx, rdy);
    }

    private static bool CanMoveTo(Critter critter, int dx, int dy, Session session)
    {
        if (dx == 0 && dy == 0) return false;

        var tile = session.Area.GetTile(critter.X + dx, critter.Y + dy);
        if (tile == null || !critter.CanEnterTile(tile))
            return false;

        return session.Area.GetBlockingCritter(critter.X + dx, critter.Y + dy) == null;
    }
}
