using Erg.Core.Combat;

namespace Erg.Core.World.Behaviors;

public class SpinAttackAction : CritterAction
{
    private readonly int _dx;
    private readonly int _dy;

    public SpinAttackAction(int dx, int dy)
    {
        _dx = dx;
        _dy = dy;
    }

    public override int EnergyCost => StandardCost;

    public override bool Execute(Critter critter, Session session)
    {
        int targetX = critter.X + _dx;
        int targetY = critter.Y + _dy;

        var tile = session.Area.GetTile(targetX, targetY);
        var target = tile?.Critter;

        if (target != null && target.IsAlive)
        {
            Combat.Combat.MeleeAttack(critter, target, session.Messages, session.Random);

            if (!target.IsAlive)
                session.Area.RemoveCritter(target);
        }
        // Jeśli brak celu - atak "w powietrze", energia i tak zużyta

        return true;
    }
}
