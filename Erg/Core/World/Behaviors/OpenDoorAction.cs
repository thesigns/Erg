namespace Erg.Core.World.Behaviors;

public class OpenDoorAction : CritterAction
{
    public int X { get; }
    public int Y { get; }

    public OpenDoorAction(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override ActionResult Execute(Critter critter, Session session)
    {
        var tile = session.Area.GetTile(X, Y);
        if (tile?.Type != TileType.ClosedDoor)
            return Failure();

        session.Area.SetTile(X, Y, Tile.OpenDoor);

        if (critter == session.Player)
        {
            session.Messages.Clear();
            session.Messages.Add("You open a door.");
        }

        return Success();
    }
}
