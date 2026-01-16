using Erg.Core.World.Critters;
using Erg.Core.World.Pathfinding;

namespace Erg.Core.World.Behaviors;

public class ZombieBehavior : IBehavior
{
    public static readonly ZombieBehavior Instance = new();

    public CritterAction DecideAction(Critter critter, Session session)
    {
        // Jeśli ma wroga
        var enemy = critter.CurrentEnemy;
        if (enemy != null)
        {
            // 0.5% szans na roztargnienie
            if (session.Random.Next(200) == 0)
            {
                critter.RemoveEnemy(enemy);
                enemy = critter.CurrentEnemy;
            }
        }

        // Jeśli nadal ma wroga
        if (enemy != null && enemy.IsAlive)
        {
            // Sąsiaduje? Atakuj
            if (IsAdjacent(critter, enemy))
                return new UnarmedAttackAction(enemy);

            // Podejdź A*
            var move = GetMoveToward(critter, enemy, session);
            return move != null ? move : WaitAction.Instance;
        }

        // Skanuj otoczenie
        for (int dist = 1; dist <= critter.SightRange; dist++)
        {
            var targets = GetTargetsAtDistance(critter, dist, session);
            if (targets.Count == 0) continue;

            var target = targets[session.Random.Next(targets.Count)];
            critter.AddEnemy(target);

            if (dist == 1)
                return new UnarmedAttackAction(target);

            var moveToTarget = GetMoveToward(critter, target, session);
            return moveToTarget != null ? moveToTarget : WaitAction.Instance;
        }

        return WaitAction.Instance;
    }

    private List<Critter> GetTargetsAtDistance(Critter critter, int dist, Session session)
    {
        var targets = new List<Critter>();
        var area = session.Area;

        // Obrys kwadratu na dystansie dist
        for (int dx = -dist; dx <= dist; dx++)
        {
            for (int dy = -dist; dy <= dist; dy++)
            {
                // Tylko obrys (krawędź kwadratu)
                if (Math.Abs(dx) != dist && Math.Abs(dy) != dist) continue;

                int x = critter.X + dx;
                int y = critter.Y + dy;

                var target = area.GetCritter(x, y);
                if (target == null || !target.IsAlive) continue;
                if (target is Zombie) continue;

                // Dla dist > 1 sprawdź czy widzi cel
                if (dist > 1 && !critter.CanSeeCritter(area, target))
                    continue;

                targets.Add(target);
            }
        }

        return targets;
    }

    private bool IsAdjacent(Critter a, Critter b)
    {
        return Math.Abs(a.X - b.X) <= 1 && Math.Abs(a.Y - b.Y) <= 1;
    }

    private MoveAction? GetMoveToward(Critter critter, Critter target, Session session)
    {
        var path = Pathfinder.FindPath(session.Area, critter, target.X, target.Y);
        if (path.Count == 0) return null;

        var (nextX, nextY) = path[0];
        return new MoveAction(nextX - critter.X, nextY - critter.Y);
    }
}
