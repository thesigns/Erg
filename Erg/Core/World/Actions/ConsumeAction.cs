namespace Erg.Core.World.Actions;

public class ConsumeAction : CritterAction
{
    private readonly Item _item;
    private readonly int _quantity;

    public ConsumeAction(Item item, int quantity = -1)
    {
        _item = item;
        _quantity = quantity;
    }

    public override ActionResult Execute(Critter critter, Session session)
    {
        if (critter == session.Player)
        {
            session.Messages.Clear();
            session.Messages.Add("You can't consume items yet.");
        }

        return Failure();
    }
}
