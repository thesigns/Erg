namespace Erg.Core.World.Actions;

/// <summary>
/// Result of executing a CritterAction.
/// </summary>
public record struct ActionResult(bool Success, int EnergyCost);

public abstract class CritterAction
{
    public const int StandardCost = 1000;

    /// <summary>
    /// Executes the action and returns the result with energy cost.
    /// </summary>
    public abstract ActionResult Execute(Critter critter, Session session);

    /// <summary>
    /// Helper to create a successful result with standard cost.
    /// </summary>
    protected static ActionResult Success() => new(true, StandardCost);

    /// <summary>
    /// Helper to create a successful result with custom cost.
    /// </summary>
    protected static ActionResult Success(int cost) => new(true, cost);

    /// <summary>
    /// Helper to create a failed result (no energy spent).
    /// </summary>
    protected static ActionResult Failure() => new(false, 0);
}
