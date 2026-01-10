namespace Erg.Core.World;

public class FieldOfView
{
    private readonly Area _area;
    private readonly bool[,] _seen;

    public int Width => _area.Width;
    public int Height => _area.Height;

    public FieldOfView(Area area)
    {
        _area = area;
        _seen = new bool[area.Width, area.Height];
    }

    public bool IsSeen(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        return _seen[x, y];
    }

    public void Compute(int originX, int originY, int radius)
    {
        // Reset seen state
        Array.Clear(_seen);

        // Origin is always visible
        _seen[originX, originY] = true;
        _area.MarkExplored(originX, originY);

        // Cast light in all 8 octants
        for (int octant = 0; octant < 8; octant++)
        {
            CastLight(originX, originY, radius, 1, 1.0, 0.0, octant);
        }
    }

    private void CastLight(int originX, int originY, int radius, int row, double startSlope, double endSlope, int octant)
    {
        if (startSlope < endSlope)
            return;

        double nextStartSlope = startSlope;

        for (int distance = row; distance <= radius; distance++)
        {
            bool blocked = false;

            int deltaY = -distance;
            for (int deltaX = -distance; deltaX <= 0; deltaX++)
            {
                // Calculate slopes
                double leftSlope = (deltaX - 0.5) / (deltaY + 0.5);
                double rightSlope = (deltaX + 0.5) / (deltaY - 0.5);

                if (startSlope < rightSlope)
                    continue;
                if (endSlope > leftSlope)
                    break;

                // Transform coordinates based on octant
                var (mapX, mapY) = TransformOctant(originX, originY, deltaX, deltaY, octant);

                // Check if in bounds and within radius
                int distanceSquared = deltaX * deltaX + deltaY * deltaY;
                if (mapX >= 0 && mapX < Width && mapY >= 0 && mapY < Height && distanceSquared <= radius * radius)
                {
                    _seen[mapX, mapY] = true;
                    _area.MarkExplored(mapX, mapY);
                }

                // Check if tile blocks light
                bool isBlocking = !IsTransparent(mapX, mapY);

                if (blocked)
                {
                    if (isBlocking)
                    {
                        nextStartSlope = rightSlope;
                    }
                    else
                    {
                        blocked = false;
                        startSlope = nextStartSlope;
                    }
                }
                else if (isBlocking && distance < radius)
                {
                    blocked = true;
                    CastLight(originX, originY, radius, distance + 1, startSlope, leftSlope, octant);
                    nextStartSlope = rightSlope;
                }
            }

            if (blocked)
                break;
        }
    }

    private (int x, int y) TransformOctant(int originX, int originY, int deltaX, int deltaY, int octant)
    {
        return octant switch
        {
            0 => (originX + deltaX, originY + deltaY),
            1 => (originX + deltaY, originY + deltaX),
            2 => (originX - deltaY, originY + deltaX),
            3 => (originX - deltaX, originY + deltaY),
            4 => (originX - deltaX, originY - deltaY),
            5 => (originX - deltaY, originY - deltaX),
            6 => (originX + deltaY, originY - deltaX),
            7 => (originX + deltaX, originY - deltaY),
            _ => (originX, originY)
        };
    }

    private bool IsTransparent(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;

        var tile = _area.GetTile(x, y);
        return tile?.Transparent ?? false;
    }
}
