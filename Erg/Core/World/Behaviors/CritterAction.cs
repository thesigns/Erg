namespace Erg.Core.World.Behaviors;

public abstract class CritterAction
{
    public const int StandardCost = 1000;

    public abstract int EnergyCost { get; }
    public abstract bool Execute(Critter critter, Session session);
}
