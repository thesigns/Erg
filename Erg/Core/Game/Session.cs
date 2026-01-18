using System;
using System.Linq;
using Erg.Core;
using Erg.Core.Combat;
using Erg.Core.Game;
using Erg.Core.Messages;
using Erg.Core.World;
using Erg.Core.World.Behaviors;
using Erg.Core.World.Critters;
using Erg.Core.World.Generators;

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
        return TryMove(Player, dx, dy);
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

        // Normal movement
        if (TryMove(Player, dx, dy))
        {
            var destTile = Area.GetTile(Player.X, Player.Y);
            int cost = (int)(CritterAction.StandardCost * Player.GetMovementCostMultiplier(destTile!));
            return MoveResult.Movement(cost);
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

    private bool TryMove(Critter critter, int dx, int dy)
    {
        int nx = critter.X + dx;
        int ny = critter.Y + dy;

        var tile = Area.GetTile(nx, ny);
        if (tile == null || !critter.CanEnterTile(tile))
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

        // Locomotion message (e.g. "You swim.", "You wade knee-deep in water.")
        var locoMessage = critter.GetLocomotionMessage(tile);
        if (locoMessage != null)
            Messages.Add(locoMessage);

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

    public void PlayerWait()
    {
        Messages.Clear();
        Messages.Add("You wait.");
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
                    action.Execute(critter, this);
                    critter.SpendEnergy(action.EnergyCost);
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