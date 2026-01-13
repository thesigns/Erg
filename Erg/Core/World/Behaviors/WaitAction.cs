namespace Erg.Core.World.Behaviors;

public class WaitAction : CritterAction
{
    public static readonly WaitAction Instance = new();

    public override int EnergyCost => StandardCost;

    public override bool Execute(Critter critter, Session session)
    {
        // W przyszlosci: regeneracja HP, efekty czekania, etc.
        return true;
    }
}
