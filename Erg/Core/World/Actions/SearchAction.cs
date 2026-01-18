namespace Erg.Core.World.Actions;

public class SearchAction : CritterAction
{
    private static readonly (int dx, int dy)[] AllDirections =
        { (-1, -1), (0, -1), (1, -1), (-1, 0), (1, 0), (-1, 1), (0, 1), (1, 1) };

    public override ActionResult Execute(Critter critter, Session session)
    {
        if (critter == session.Player)
        {
            session.Messages.Clear();
            session.Messages.Add("You search the area around you.");
        }

        bool success = session.Random.Next(100) < critter.Searching;

        if (!success)
        {
            if (critter == session.Player)
                session.Messages.Add("You haven't found anything... yet.");
            return Success(); // Action still succeeds (turn consumed)
        }

        bool foundSomething = false;
        foreach (var (dx, dy) in AllDirections)
        {
            int nx = critter.X + dx;
            int ny = critter.Y + dy;
            var tile = session.Area.GetTile(nx, ny);
            if (tile?.Type == TileType.SecretDoor)
            {
                session.Area.SetTile(nx, ny, Tile.ClosedDoor);
                if (critter == session.Player)
                    session.Messages.Add("You found a secret door!");
                foundSomething = true;
            }
        }

        if (!foundSomething && critter == session.Player)
            session.Messages.Add("You haven't found anything... yet.");

        return Success();
    }
}
