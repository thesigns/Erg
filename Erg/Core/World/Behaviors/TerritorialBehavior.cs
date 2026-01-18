using Erg.Core.World.Pathfinding;

namespace Erg.Core.World.Behaviors;

/// <summary>
/// Generic territorial behavior for creatures that guard a spawn area.
/// </summary>
public class TerritorialBehavior : IBehavior
{
    private static readonly (int dx, int dy)[] Directions =
    {
        (-1, -1), (0, -1), (1, -1),
        (-1,  0),          (1,  0),
        (-1,  1), (0,  1), (1,  1)
    };

    private readonly double _forgetChance;
    private readonly int _territoryRadius;

    private int _spawnX;
    private int _spawnY;
    private bool _initialized;

    /// <summary>
    /// Creates a new territorial behavior.
    /// </summary>
    /// <param name="forgetChance">Chance (0-1) to forget an enemy not in sight each turn.</param>
    /// <param name="territoryRadius">Radius around spawn point for wandering.</param>
    public TerritorialBehavior(double forgetChance = 0.1, int territoryRadius = 5)
    {
        _forgetChance = forgetChance;
        _territoryRadius = territoryRadius;
    }

    public void OnSpawn(Critter critter, Area area)
    {
        _spawnX = critter.X;
        _spawnY = critter.Y;
        _initialized = true;
    }

    public CritterAction DecideAction(Critter critter, Session session)
    {
        // Fallback if OnSpawn wasn't called
        if (!_initialized)
        {
            _spawnX = critter.X;
            _spawnY = critter.Y;
            _initialized = true;
        }

        var area = session.Area;
        var random = session.Random;

        // 1. Forget enemies not in sight
        ForgetInvisibleEnemies(critter, area, random);

        // 2. Check for Player if no enemies
        if (critter.CurrentEnemy == null)
        {
            var player = session.Player;
            if (player.IsAlive && critter.CanSeeCritter(area, player))
            {
                critter.AddEnemy(player);
            }
        }

        // 3. Handle enemies
        var enemy = critter.CurrentEnemy;
        if (enemy != null && enemy.IsAlive)
        {
            // Gather all visible enemies and find closest
            var visibleEnemies = critter.Enemies
                .Where(e => e.IsAlive && critter.CanSeeCritter(area, e))
                .ToList();

            if (visibleEnemies.Count > 0)
            {
                var closest = visibleEnemies
                    .OrderBy(e => GetChebyshevDistance(critter.X, critter.Y, e.X, e.Y))
                    .First();

                // Adjacent? Attack
                if (IsAdjacent(critter, closest))
                    return new UnarmedAttackAction(closest);

                // Approach
                var move = GetMoveToward(critter, closest.X, closest.Y, session);
                if (move != null)
                    return move;
            }
        }

        // 4. No visible enemies - check if need to return to territory
        int distFromSpawn = GetChebyshevDistance(critter.X, critter.Y, _spawnX, _spawnY);
        if (distFromSpawn > _territoryRadius)
        {
            var returnMove = GetMoveToward(critter, _spawnX, _spawnY, session);
            if (returnMove != null)
                return returnMove;
        }

        // 5. Wander within territory
        var wanderMove = GetRandomTerritoryMove(critter, session);
        if (wanderMove != null)
            return wanderMove;

        return WaitAction.Instance;
    }

    private void ForgetInvisibleEnemies(Critter critter, Area area, Random random)
    {
        foreach (var enemy in critter.Enemies.ToList())
        {
            if (!enemy.IsAlive)
            {
                critter.RemoveEnemy(enemy);
                continue;
            }

            if (!critter.CanSeeCritter(area, enemy))
            {
                if (random.NextDouble() < _forgetChance)
                    critter.RemoveEnemy(enemy);
            }
        }
    }

    private MoveAction? GetMoveToward(Critter critter, int targetX, int targetY, Session session)
    {
        var path = Pathfinder.FindPath(session.Area, critter, targetX, targetY);
        if (path.Count == 0) return null;

        var (nextX, nextY) = path[0];
        return new MoveAction(nextX - critter.X, nextY - critter.Y);
    }

    private MoveAction? GetRandomTerritoryMove(Critter critter, Session session)
    {
        var preferred = new List<(int dx, int dy)>();
        var other = new List<(int dx, int dy)>();
        var area = session.Area;

        foreach (var (dx, dy) in Directions)
        {
            int nx = critter.X + dx;
            int ny = critter.Y + dy;

            if (GetChebyshevDistance(nx, ny, _spawnX, _spawnY) > _territoryRadius)
                continue;

            var tile = area.GetTile(nx, ny);
            if (tile == null || !critter.CanEnterTile(tile))
                continue;

            if (area.GetBlockingCritter(nx, ny) != null)
                continue;

            // Use movement cost to determine terrain preference
            if (critter.GetMovementCostMultiplier(tile) <= 1.0f)
                preferred.Add((dx, dy));
            else
                other.Add((dx, dy));
        }

        var moves = preferred.Count > 0 ? preferred : other;
        if (moves.Count == 0)
            return null;

        var (rdx, rdy) = moves[session.Random.Next(moves.Count)];
        return new MoveAction(rdx, rdy);
    }

    private static bool IsAdjacent(Critter a, Critter b)
    {
        return Math.Abs(a.X - b.X) <= 1 && Math.Abs(a.Y - b.Y) <= 1;
    }

    private static int GetChebyshevDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
    }
}
