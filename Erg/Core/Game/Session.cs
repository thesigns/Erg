using System;
using System.Linq;
using Erg.Core;
using Erg.Core.Combat;
using Erg.Core.Game;
using Erg.Core.Messages;
using Erg.Core.World;
using Erg.Core.World.Actions;
using Erg.Core.World.Behaviors;
using Erg.Core.World.Critters;
using Erg.Core.World.Generators;
using Erg.Core.World.Items;

public struct MoveResult
{
    public bool Moved;
    public bool Attacked;
    public bool NeedsConfirmation;
    public Critter? Target;
    public int EnergyCost;

    public static MoveResult Movement(int energyCost = 1000) =>
        new() { Moved = true, EnergyCost = energyCost };
    public static MoveResult Attack() => new() { Attacked = true, EnergyCost = 1000 };
    public static MoveResult Blocked() => new();
    public static MoveResult ConfirmAttack(Critter target) =>
        new() { NeedsConfirmation = true, Target = target };
}

public enum DoorCloseResult { Success, NoDoor, Blocked }

public class Session
{
    public Random Random { get; }
    public Area Area { get; private set; }
    public Player Player { get; }
    public FieldOfView Fov { get; private set; }
    public MessageBuffer Messages { get; } = new();
    public int CurrentDepth => Area.Depth;
    public int TurnCount { get; private set; }
    public int DeepestDepth { get; private set; } = 1;

    /// <summary>
    /// Globalny mnożnik szybkości treningu atrybutów.
    /// 1.0 = normalna szybkość, 0.5 = dwukrotnie wolniej, 2.0 = dwukrotnie szybciej.
    /// </summary>
    public double TrainingSpeed { get; set; } = 1.0;

    public Session(SessionConfig config)
    {
        Random = new Random(config.Seed);

        var generator = new DungeonGenerator3(80, 20, Random.Next(), level: 1);
        Area = generator.Generate();

        var (startX, startY) = generator.GetPlayerStartPosition();
        Player = new Player(startX, startY);
        Player.DepthTraining(Area.Depth, Random);
        Player.FullHeal();
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
        Fov.Compute(Player.X, Player.Y, Player.SightRange);
    }

    public bool CanPlayerSee(Critter critter)
    {
        // Player always sees themselves
        if (critter == Player)
            return true;

        // Check FOV visibility
        // Future: also check critter.IsInvisible flag
        return Fov.IsSeen(critter.X, critter.Y);
    }

    public void IncrementTurn()
    {
        TurnCount++;
    }

    public bool TryMovePlayer(int dx, int dy)
    {
        var moveAction = new MoveAction(dx, dy);
        return moveAction.Execute(Player, this).Success;
    }

    public MoveResult TryMoveOrAttack(int dx, int dy)
    {
        int nx = Player.X + dx;
        int ny = Player.Y + dy;

        var tile = Area.GetTile(nx, ny);
        if (tile == null)
            return MoveResult.Blocked();

        // Auto-open closed doors (not secret)
        if (!Player.CanEnterTile(tile) && tile.Type == TileType.ClosedDoor && Player.CanOpenDoor)
        {
            TryOpenDoorAt(nx, ny);
            ComputeFov();
            return MoveResult.Movement();
        }

        if (!Player.CanEnterTile(tile))
            return MoveResult.Blocked();

        var blocker = Area.GetBlockingCritter(nx, ny);
        if (blocker != null)
        {
            // Hostile critter - attack immediately
            if (blocker.Enemies.Contains(Player))
            {
                PlayerAttack(blocker);
                return MoveResult.Attack();
            }

            // Non-hostile - ask for confirmation
            Messages.Clear();
            Messages.Add($"{blocker.Name} isn't hostile. Do you want to attack {blocker.Pronouns.Object}? [y/n]");
            return MoveResult.ConfirmAttack(blocker);
        }

        // Normal movement - use MoveAction for unified logic
        var moveAction = new MoveAction(dx, dy);
        var result = moveAction.Execute(Player, this);

        if (result.Success)
        {
            // Passive search is now handled in MoveAction.Execute()
            return MoveResult.Movement(result.EnergyCost);
        }

        return MoveResult.Blocked();
    }

    public void PlayerAttack(Critter target)
    {
        Messages.Clear();
        Combat.UnarmedAttack(Player, target, this);

        if (!target.IsAlive)
        {
            target.OnDeath(Area);
            Area.RemoveCritter(target);
        }
    }

    public void Examine(int dx, int dy)
    {
        Messages.Clear();
        ExamineAt(Player.X + dx, Player.Y + dy);
    }

    public void ExamineAt(int x, int y)
    {
        var tile = Area.GetTile(x, y);
        if (tile == null)
        {
            Messages.Add("There is nothing there.");
            return;
        }

        Messages.Add(BuildExamineDescription(tile));
    }

    private string BuildExamineDescription(Tile tile)
    {
        var parts = new List<string>();

        // 1. Tile name
        parts.Add($"This is {GetArticle(tile.Name)} {tile.Name}.");

        // 2. Inscription (if any)
        if (!string.IsNullOrEmpty(tile.Inscription))
            parts.Add($"There is an inscription: \"{tile.Inscription}\"");

        // 3. Critter (if any)
        if (tile.Critter != null)
        {
            if (tile.Critter == Player)
                parts.Add("You see yourself standing here. Looking good!");
            else
            {
                parts.Add($"There is {GetArticle(tile.Critter.Name)} {tile.Critter.Name} here.");

                // Sprawdź relację do gracza
                if (tile.Critter.Enemies.Contains(Player))
                    parts.Add($"{tile.Critter.Pronouns.Subject} is hostile.");
                else
                    parts.Add($"{tile.Critter.Pronouns.Subject} doesn't care about you.");

                // Sprawdź zdrowie
                if (tile.Critter.HitPoints < tile.Critter.MaxHitPoints)
                    parts.Add($"{tile.Critter.Pronouns.Subject} is wounded.");
            }
        }

        // 4. Items (if any)
        if (tile.Items.Count > 0)
        {
            parts.Add(DescribeItems(tile.Items));
        }

        return string.Join(" ", parts);
    }

    private static string GetArticle(string noun)
    {
        if (string.IsNullOrEmpty(noun)) return "a";
        char first = char.ToLower(noun[0]);
        return "aeiou".Contains(first) ? "an" : "a";
    }

    private static string DescribeItems(List<Item> items)
    {
        if (items.Count == 1)
        {
            var item = items[0];
            if (item.Count > 1)
                return $"There are {item.Count} {item.Name}s lying here.";
            return $"There is {GetArticle(item.Name)} {item.Name} lying here.";
        }
        return "There are several items lying here.";
    }

    private static readonly (int dx, int dy)[] AllDirections =
    {
        (-1, -1), (0, -1), (1, -1),
        (-1,  0),          (1,  0),
        (-1,  1), (0,  1), (1,  1)
    };

    public int CountAdjacentClosedDoors()
    {
        int count = 0;
        foreach (var (dx, dy) in AllDirections)
        {
            var tile = Area.GetTile(Player.X + dx, Player.Y + dy);
            if (tile?.Type == TileType.ClosedDoor) count++;
        }
        return count;
    }

    public int CountAdjacentOpenDoors()
    {
        int count = 0;
        foreach (var (dx, dy) in AllDirections)
        {
            var tile = Area.GetTile(Player.X + dx, Player.Y + dy);
            if (tile?.Type == TileType.OpenDoor) count++;
        }
        return count;
    }

    public bool TryOpenDoorAt(int x, int y)
    {
        var tile = Area.GetTile(x, y);
        if (tile?.Type == TileType.ClosedDoor)
        {
            Area.SetTile(x, y, Tile.OpenDoor);
            Messages.Clear();
            Messages.Add("You open a door.");
            return true;
        }
        return false;
    }

    public DoorCloseResult TryCloseDoorAt(int x, int y)
    {
        var tile = Area.GetTile(x, y);
        if (tile?.Type != TileType.OpenDoor)
            return DoorCloseResult.NoDoor;

        if (tile.Critter != null || tile.Items.Count > 0)
        {
            Messages.Clear();
            Messages.Add("Something blocks the door.");
            return DoorCloseResult.Blocked;
        }

        Area.SetTile(x, y, Tile.ClosedDoor);
        Messages.Clear();
        Messages.Add("You close a door.");
        return DoorCloseResult.Success;
    }

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

    public bool CloseAdjacentDoors()
    {
        Messages.Clear();
        bool anySuccess = false;

        foreach (var (dx, dy) in AllDirections)
        {
            int nx = Player.X + dx;
            int ny = Player.Y + dy;
            var tile = Area.GetTile(nx, ny);
            if (tile?.Type == TileType.OpenDoor)
            {
                if (tile.Critter != null || tile.Items.Count > 0)
                {
                    Messages.Add("Something blocks the door.");
                }
                else
                {
                    Area.SetTile(nx, ny, Tile.ClosedDoor);
                    Messages.Add("You close a door.");
                    anySuccess = true;
                }
            }
        }
        return anySuccess;
    }

    public void ReadBook(Book book)
    {
        Messages.Clear();

        // Can't read if Reading is 0
        if (Player.Reading == 0)
        {
            Messages.Add("You can't read.");
            return;
        }

        bool consumed = book.OnRead(Player, this);

        // Train Literacy from reading practice
        Player.TrainLiteracy(0.01, this);

        // Message about reading
        Messages.Add($"You read {book.Name}.");

        // Message based on Reading level
        int reading = Player.Reading;
        string comprehension = reading switch
        {
            < 20 => "You barely understood anything.",
            < 40 => "You understood some of it.",
            < 60 => "You grasped most of the content.",
            < 80 => "You understood it well.",
            _ => "You fully comprehended the text."
        };
        Messages.Add(comprehension);

        // Remove if consumed
        if (consumed)
        {
            Player.Inventory.Remove(book);
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
        int newDepth = Area.Depth + 1;
        if (newDepth > DeepestDepth)
            DeepestDepth = newDepth;
        RegenerateArea(newDepth, startOnStairsUp: true);
    }

    public void GoUpStairs()
    {
        int newDepth = Area.Depth - 1;
        RegenerateArea(newDepth, startOnStairsUp: false);
    }

    private void RegenerateArea(int newDepth, bool startOnStairsUp)
    {
        var generator = new DungeonGenerator3(80, 20, Random.Next(), level: newDepth);
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

    public void ProcessCritterTurns(int playerActionCost = 1000)
    {
        int segments = playerActionCost / Player.EnergyRegenRate;
        var critters = GetAllCritters().Where(c => c != Player).ToList();

        for (int seg = 0; seg < segments; seg++)
        {
            // Player regeneration (accumulates during NPC turns)
            Player.TryAccumulateRegen(Random);

            foreach (var critter in critters)
            {
                critter.GainEnergy();
                critter.TryAccumulateRegen(Random);

                while (critter.CanAct())
                {
                    critter.ApplyPendingRegen();

                    if (critter.Behavior == null)
                    {
                        critter.SpendEnergy(CritterAction.StandardCost);
                        break;
                    }

                    var action = critter.Behavior.DecideAction(critter, this);
                    var result = action.Execute(critter, this);
                    critter.SpendEnergy(result.EnergyCost);
                }
            }
        }
    }

    private IEnumerable<Critter> GetAllCritters()
    {
        for (int y = 0; y < Area.Height; y++)
        {
            for (int x = 0; x < Area.Width; x++)
            {
                var critter = Area.GetCritter(x, y);
                if (critter != null)
                    yield return critter;
            }
        }
    }
}