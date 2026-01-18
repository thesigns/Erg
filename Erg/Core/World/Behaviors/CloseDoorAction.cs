namespace Erg.Core.World.Behaviors;

public class CloseDoorAction : CritterAction
{
    public int X { get; }
    public int Y { get; }

    public CloseDoorAction(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override ActionResult Execute(Critter critter, Session session)
    {
        var tile = session.Area.GetTile(X, Y);
        if (tile?.Type != TileType.OpenDoor)
            return Failure();

        if (tile.Critter != null || tile.Items.Count > 0)
        {
            if (critter == session.Player)
            {
                session.Messages.Clear();
                session.Messages.Add("Something blocks the door.");
            }
            return Failure();
        }

        session.Area.SetTile(X, Y, Tile.ClosedDoor);

        if (critter == session.Player)
        {
            session.Messages.Clear();
            session.Messages.Add("You close a door.");
        }

        return Success();
    }
}
