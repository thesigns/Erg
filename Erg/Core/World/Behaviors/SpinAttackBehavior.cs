namespace Erg.Core.World.Behaviors;

public class SpinAttackBehavior : IBehavior
{
    // Kierunki zgodnie z ruchem wskazówek zegara: N, NE, E, SE, S, SW, W, NW
    private static readonly (int dx, int dy)[] Directions =
    [
        (0, -1),   // N
        (1, -1),   // NE
        (1, 0),    // E
        (1, 1),    // SE
        (0, 1),    // S
        (-1, 1),   // SW
        (-1, 0),   // W
        (-1, -1)   // NW
    ];

    private int _currentDirection;

    public CritterAction DecideAction(Critter critter, Session session)
    {
        var (dx, dy) = Directions[_currentDirection];
        _currentDirection = (_currentDirection + 1) % 8;
        return new SpinAttackAction(dx, dy);
    }
}
