using Erg.Core.Combat;

namespace Erg.Core.World.Behaviors;

public class MeleeAttackAction : CritterAction
{
    public Critter Target { get; }

    public MeleeAttackAction(Critter target)
    {
        Target = target;
    }

    public override int EnergyCost => StandardCost;

    public override bool Execute(Critter critter, Session session)
    {
        Combat.Combat.MeleeAttack(critter, Target, session);

        if (!Target.IsAlive)
        {
            critter.RemoveEnemy(Target);
            Target.OnDeath(session.Area);
            session.Area.RemoveCritter(Target);
        }

        return true;
    }
}
