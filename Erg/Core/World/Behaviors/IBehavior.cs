namespace Erg.Core.World.Behaviors;

public interface IBehavior
{
    CritterAction DecideAction(Critter critter, Session session);
}
