namespace Erg.Core.World.Actions;

public class MoveAction : CritterAction
{
    private static readonly (int dx, int dy)[] AllDirections =
        { (-1, -1), (0, -1), (1, -1), (-1, 0), (1, 0), (-1, 1), (0, 1), (1, 1) };

    public int Dx { get; }
    public int Dy { get; }

    public MoveAction(int dx, int dy)
    {
        Dx = dx;
        Dy = dy;
    }

    public override ActionResult Execute(Critter critter, Session session)
    {
        int nx = critter.X + Dx;
        int ny = critter.Y + Dy;
        var tile = session.Area.GetTile(nx, ny);

        if (tile == null || !critter.CanEnterTile(tile))
            return Failure();

        if (session.Area.GetBlockingCritter(nx, ny) != null)
            return Failure();

        // Save old SpecialEffect before moving (for player messages)
        var oldTile = session.Area.GetTile(critter.X, critter.Y);
        var oldEffect = oldTile?.SpecialEffect ?? SpecialEffect.None;

        session.Area.MoveCritter(critter, nx, ny);

        // Calculate energy cost based on terrain
        int cost = (int)(StandardCost * critter.GetMovementCostMultiplier(tile));

        // Player-specific: generate messages
        if (critter == session.Player)
        {
            session.Messages.Clear();

            // Locomotion message (e.g. "You swim.", "You wade knee-deep in water.")
            var locoMessage = critter.GetLocomotionMessage(tile);
            if (locoMessage != null)
                session.Messages.Add(locoMessage);

            // SpecialEffect message if entering new effect zone
            var newEffect = tile.SpecialEffect;
            if (newEffect != oldEffect && newEffect != SpecialEffect.None)
            {
                session.Messages.Add(GetSpecialEffectMessage(newEffect));
            }

            // Tile name
            session.Messages.Add($"{tile.Name}.");

            // Item notifications
            var items = session.Area.GetItems(nx, ny);
            if (items.Count > 1)
            {
                session.Messages.Add("Several items are lying here.");
            }
            else if (items.Count == 1)
            {
                var item = items[0];
                if (item.Count > 1)
                    session.Messages.Add($"{item.Count} {item.Name}s are lying here.");
                else
                    session.Messages.Add($"{item.Name} is lying here.");
            }
        }

        // Train swimming when moving through deep water (all critters)
        if (tile.Type == TileType.DeepWater)
            critter.TrainSwimmingPractice(0.001, session);

        // Passive search while moving (1/20 of normal chance) - player only
        if (critter == session.Player)
        {
            if (session.Random.Next(100) < critter.Searching / 20)
            {
                foreach (var (dx, dy) in AllDirections)
                {
                    int sx = critter.X + dx;
                    int sy = critter.Y + dy;
                    var searchTile = session.Area.GetTile(sx, sy);
                    if (searchTile?.Type == TileType.SecretDoor)
                    {
                        session.Area.SetTile(sx, sy, Tile.ClosedDoor);
                        session.Messages.Add("You notice a secret door!");
                    }
                }
            }
        }

        return Success(cost);
    }

    private static string GetSpecialEffectMessage(SpecialEffect effect) => effect switch
    {
        SpecialEffect.UndeadAura => "A chill of death hangs over this place.",
        _ => ""
    };
}
