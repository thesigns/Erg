namespace Erg.Core.World.Behaviors;

public class PassiveBehavior : IBehavior
{
    public static readonly PassiveBehavior Instance = new();

    public CritterAction DecideAction(Critter critter, Session session)
    {
        return WaitAction.Instance;
    }
}
