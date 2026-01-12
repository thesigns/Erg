using System;
using Erg.Core.Game;
using Erg.Core.World;
using Erg.Core.World.Generators;

public class Session
{
    public Random Random { get; }
    public Area Area { get; private set; }
    public Player Player { get; }
    public FieldOfView Fov { get; private set; }
    public MessageBuffer Messages { get; } = new();
    public int CurrentLevel => Area.Level;

    private const int ViewRadius = 10;

    public Session(SessionConfig config)
    {
        Random = new Random(config.Seed);

        var generator = new DungeonGenerator3(80, 20, Random, level: 1);
        Area = generator.Generate();

        var (startX, startY) = generator.GetPlayerStartPosition();
        Player = new Player(startX, startY);
        Area.SetCritter(Player);

        Fov = new FieldOfView(Area);
        ComputeFov();

        var startTile = Area.GetTile(Player.X, Player.Y);
        if (startTile != null)
        {
            Messages.Add($"{startTile.Name}.");
        }
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

        // Zapisz stary SpecialEffect przed ruchem
        var oldTile = Area.GetTile(critter.X, critter.Y);
        var oldEffect = oldTile?.SpecialEffect ?? SpecialEffect.None;

        Area.MoveCritter(critter, nx, ny);

        // Wyczyść stare wiadomości z poprzedniej tury
        Messages.Clear();

        // Sprawdź czy wchodzimy na kafelek z innym SpecialEffect
        var newEffect = tile.SpecialEffect;
        if (newEffect != oldEffect && newEffect != SpecialEffect.None)
        {
            Messages.Add(GetSpecialEffectMessage(newEffect));
        }

        Messages.Add($"{tile.Name}.");

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

    private static string GetSpecialEffectMessage(SpecialEffect effect) => effect switch
    {
        SpecialEffect.UndeadAura => "A chill of death hangs over this place.",
        _ => ""
    };

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
            if (tile?.Type == TileType.ClosedDoor)
            {
                Area.SetTile(nx, ny, Tile.OpenDoor);
                Messages.Add("You open a door.");
            }
            else if (tile?.Type == TileType.SecretDoor)
            {
                Area.SetTile(nx, ny, Tile.Floor(TileStructure.Entrance));
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
            if (tile?.Type == TileType.OpenDoor)
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

    public bool IsPlayerOnStairsDown()
    {
        var tile = Area.GetTile(Player.X, Player.Y);
        return tile?.Type == TileType.StairsDown;
    }

    public bool IsPlayerOnStairsUp()
    {
        var tile = Area.GetTile(Player.X, Player.Y);
        return tile?.Type == TileType.StairsUp;
    }

    public void GoDownStairs()
    {
        int newLevel = Area.Level + 1;
        RegenerateArea(newLevel, startOnStairsUp: true);
    }

    public void GoUpStairs()
    {
        int newLevel = Area.Level - 1;
        RegenerateArea(newLevel, startOnStairsUp: false);
    }

    private void RegenerateArea(int newLevel, bool startOnStairsUp)
    {
        var generator = new DungeonGenerator3(80, 20, Random, level: newLevel);
        Area = generator.Generate();

        var startPos = startOnStairsUp
            ? FindTileOfType(TileType.StairsUp)
            : FindTileOfType(TileType.StairsDown);

        Player.MoveTo(startPos.x, startPos.y);
        Area.SetCritter(Player);

        Fov = new FieldOfView(Area);
        ComputeFov();

        Messages.Clear();
        var startTile = Area.GetTile(Player.X, Player.Y);
        if (startTile != null)
        {
            Messages.Add($"{startTile.Name}.");
        }
    }

    private (int x, int y) FindTileOfType(TileType type)
    {
        for (int y = 0; y < Area.Height; y++)
            for (int x = 0; x < Area.Width; x++)
                if (Area.GetTile(x, y)?.Type == type)
                    return (x, y);
        return (0, 0);
    }
}