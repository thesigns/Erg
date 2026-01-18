namespace Erg.Core.World.Behaviors;

public interface IBehavior
{
    CritterAction DecideAction(Critter critter, Session session);

    /// <summary>
    /// Called after critter is placed in the area. Use for spawn-time initialization.
    /// </summary>
    void OnSpawn(Critter critter, Area area) { }
}
