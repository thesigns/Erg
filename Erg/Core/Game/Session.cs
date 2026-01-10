using Erg.Core.Game;
using Erg.Core.World;

public class Session
{
    public Random Random { get; }
    public Area Area { get; }
    public Player Player { get; }
    public FieldOfView Fov { get; }

    private const int ViewRadius = 10;

    public Session(SessionConfig config)
    {
        Random = new Random(config.Seed);

        var generator = new DungeonGenerator(80, 20, Random);
        Area = generator.Generate();

        var (startX, startY) = generator.GetPlayerStartPosition();
        Player = new Player(startX, startY);
        Area.AddEntity(Player);

        Fov = new FieldOfView(Area);
        ComputeFov();
    }

    public void ComputeFov()
    {
        Fov.Compute(Player.X, Player.Y, ViewRadius);
    }

    public bool TryMovePlayer(int dx, int dy)
    {
        return TryMove(Player, dx, dy);
    }

    private bool TryMove(Critter critter, int dx, int dy)
    {
        int nx = critter.X + dx;
        int ny = critter.Y + dy;

        var tile = Area.GetTile(nx, ny);
        if (tile == null || !tile.Walkable)
            return false;

        var blocker = Area.GetBlockingCritter(nx, ny);
        if (blocker != null)
            return false; // na razie nic więcej

        critter.MoveTo(nx, ny);
        return true;
    }

    private static readonly (int dx, int dy)[] AllDirections =
    {
        (-1, -1), (0, -1), (1, -1),
        (-1,  0),          (1,  0),
        (-1,  1), (0,  1), (1,  1)
    };

    public void OpenAdjacentDoors()
    {
        foreach (var (dx, dy) in AllDirections)
        {
            int nx = Player.X + dx;
            int ny = Player.Y + dy;
            var tile = Area.GetTile(nx, ny);
            if (tile?.Name == "Closed Door")
            {
                Area.SetTile(nx, ny, Tile.OpenDoor);
            }
        }
    }

    public void CloseAdjacentDoors()
    {
        foreach (var (dx, dy) in AllDirections)
        {
            int nx = Player.X + dx;
            int ny = Player.Y + dy;
            var tile = Area.GetTile(nx, ny);
            if (tile?.Name == "Open Door")
            {
                Area.SetTile(nx, ny, Tile.ClosedDoor);
            }
        }
    }
}