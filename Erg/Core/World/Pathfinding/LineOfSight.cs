namespace Erg.Core.World.Pathfinding;

public static class LineOfSight
{
    /// <summary>
    /// Sprawdza czy z (x1,y1) widać (x2,y2).
    /// Sprawdza oba kierunki żeby uniknąć asymetrii tile-based map.
    /// </summary>
    public static bool CanSee(Area area, int x1, int y1, int x2, int y2)
    {
        return HasLineOfSight(area, x1, y1, x2, y2)
            || HasLineOfSight(area, x2, y2, x1, y1);
    }

    /// <summary>
    /// Jednokierunkowy LOS używając algorytmu Bresenhama.
    /// </summary>
    private static bool HasLineOfSight(Area area, int x1, int y1, int x2, int y2)
    {
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx - dy;

        int x = x1;
        int y = y1;

        while (true)
        {
            // Pomijamy punkt startowy i końcowy
            if ((x != x1 || y != y1) && (x != x2 || y != y2))
            {
                var tile = area.GetTile(x, y);
                if (tile == null || !tile.Transparent)
                    return false;
            }

            if (x == x2 && y == y2)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        return true;
    }
}
