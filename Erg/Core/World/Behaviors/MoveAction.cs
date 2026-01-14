namespace Erg.Core.World.Behaviors;

public class MoveAction : CritterAction
{
    public int Dx { get; }
    public int Dy { get; }

    public MoveAction(int dx, int dy)
    {
        Dx = dx;
        Dy = dy;
    }

    public override int EnergyCost => StandardCost;

    public override bool Execute(Critter critter, Session session)
    {
        int nx = critter.X + Dx;
        int ny = critter.Y + Dy;
        var tile = session.Area.GetTile(nx, ny);

        if (tile == null || !critter.CanEnterTile(tile))
            return false;

        if (session.Area.GetBlockingCritter(nx, ny) != null)
            return false;

        session.Area.MoveCritter(critter, nx, ny);
        return true;
    }
}
