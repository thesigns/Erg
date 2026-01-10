using System;
using Erg.Core.Game;
using Erg.Core.World;
using Erg.Core.World.Generators;

public class Session
{
    public Random Random { get; }
    public Area Area { get; }
    public Player Player { get; }
    public FieldOfView Fov { get; }
    public MessageBuffer Messages { get; } = new();

    private const int ViewRadius = 10;

    public Session(SessionConfig config)
    {
        Random = new Random(config.Seed);

        var generator = new DungeonGenerator3(80, 20, Random);
        Area = generator.Generate();

        var (startX, startY) = generator.GetPlayerStartPosition();
        Player = new Player(startX, startY);
        Area.SetCritter(Player);

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

        Area.MoveCritter(critter, nx, ny);

        // Wyczyść stare wiadomości z poprzedniej tury
        Messages.Clear();

        // Sprawdź itemy na nowej pozycji
        var items = Area.GetItems(nx, ny);
        if (items.Count > 1)
        {
            Messages.Add("Several items are lying here.");
        }
        else if (items.Count == 1)
        {
            var item = items[0];
            if (item.Count > 1)
                Messages.Add($"{item.Count} {item.Name}s are lying here.");
            else
                Messages.Add($"{item.Name} is lying here.");
        }

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
        Messages.Clear();
        foreach (var (dx, dy) in AllDirections)
        {
            int nx = Player.X + dx;
            int ny = Player.Y + dy;
            var tile = Area.GetTile(nx, ny);
            if (tile?.Name == "Closed Door")
            {
                Area.SetTile(nx, ny, Tile.OpenDoor);
                Messages.Add("You open a door.");
            }
            else if (tile?.Name == "Secret Door")
            {
                Area.SetTile(nx, ny, Tile.EntranceFloor);
                Messages.Add("You discover a secret passage!");
            }
        }
    }

    public void CloseAdjacentDoors()
    {
        Messages.Clear();
        foreach (var (dx, dy) in AllDirections)
        {
            int nx = Player.X + dx;
            int ny = Player.Y + dy;
            var tile = Area.GetTile(nx, ny);
            if (tile?.Name == "Open Door")
            {
                Area.SetTile(nx, ny, Tile.ClosedDoor);
                Messages.Add("You close a door.");
            }
        }
    }

    public void PickUpItems()
    {
        Messages.Clear();

        var tile = Area.GetTile(Player.X, Player.Y);
        if (tile == null || tile.Items.Count == 0)
        {
            Messages.Add("There is nothing here to pick up.");
            return;
        }

        foreach (var item in tile.Items.ToList())
        {
            Player.Inventory.Add(item);
            Area.RemoveItem(item);

            if (item.Count > 1)
                Messages.Add($"You pick up {item.Count} {item.Name}s.");
            else
                Messages.Add($"You pick up {item.Name}.");

        }
    }
}