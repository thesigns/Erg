namespace Erg.Core.World.Actions;

public class DropAction : CritterAction
{
    private readonly Item _item;
    private readonly int _quantity;

    public DropAction(Item item, int quantity = -1)
    {
        _item = item;
        _quantity = quantity;
    }

    public override ActionResult Execute(Critter critter, Session session)
    {
        if (!critter.Inventory.Items.Contains(_item))
            return Failure();

        int toDrop = _quantity <= 0 || _quantity >= _item.Count
            ? _item.Count
            : _quantity;

        Item droppedItem;

        if (toDrop >= _item.Count)
        {
            critter.Inventory.Remove(_item);
            _item.MoveTo(critter.X, critter.Y);
            droppedItem = _item;
        }
        else
        {
            _item.Count -= toDrop;
            droppedItem = _item.SplitStack(toDrop, critter.X, critter.Y);
        }

        session.Area.AddItem(droppedItem);

        if (critter == session.Player)
        {
            session.Messages.Clear();
            if (toDrop > 1)
                session.Messages.Add($"You drop {toDrop} {droppedItem.Name}s.");
            else
                session.Messages.Add($"You drop the {droppedItem.Name}.");
        }

        return Success();
    }
}
