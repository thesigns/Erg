namespace Erg.Core.World.Behaviors;

public class ZombieBehavior : IBehavior
{
    public static readonly ZombieBehavior Instance = new();

    public CritterAction DecideAction(Critter critter, Session session)
    {
        return WaitAction.Instance;
    }
}
