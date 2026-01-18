using Erg.Core.Combat;

namespace Erg.Core.World.Actions;

public class UnarmedAttackAction : CritterAction
{
    public Critter Target { get; }

    public UnarmedAttackAction(Critter target)
    {
        Target = target;
    }

    public override ActionResult Execute(Critter critter, Session session)
    {
        Combat.Combat.UnarmedAttack(critter, Target, session);

        if (!Target.IsAlive)
        {
            critter.RemoveEnemy(Target);
            Target.OnDeath(session.Area);
            session.Area.RemoveCritter(Target);
        }

        return Success();
    }
}
