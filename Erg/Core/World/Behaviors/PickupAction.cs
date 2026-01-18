namespace Erg.Core.World.Behaviors;

public class PickupAction : CritterAction
{
    public override ActionResult Execute(Critter critter, Session session)
    {
        var tile = session.Area.GetTile(critter.X, critter.Y);
        if (tile == null || tile.Items.Count == 0)
        {
            if (critter == session.Player)
            {
                session.Messages.Clear();
                session.Messages.Add("There is nothing here to pick up.");
            }
            return Failure();
        }

        if (critter == session.Player)
            session.Messages.Clear();

        foreach (var item in tile.Items.ToList())
        {
            critter.Inventory.Add(item);
            session.Area.RemoveItem(item);

            if (critter == session.Player)
            {
                if (item.Count > 1)
                    session.Messages.Add($"You pick up {item.Count} {item.Name}s.");
                else
                    session.Messages.Add($"You pick up {item.Name}.");
            }
        }

        return Success();
    }
}
