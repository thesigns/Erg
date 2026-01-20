namespace Erg.Core.World.Actions;

public class WieldAction : CritterAction
{
    private readonly Item _item;

    public WieldAction(Item item)
    {
        _item = item;
    }

    public override ActionResult Execute(Critter critter, Session session)
    {
        if (critter == session.Player)
        {
            session.Messages.Clear();
            session.Messages.Add("You can't wield items yet.");
        }

        return Failure();
    }
}
