namespace Erg.Core.Game;

public class SessionConfig
{
    public int Seed { get; init; }

    private SessionConfig(int seed)
    {
        Seed = seed;
    }

    public static SessionConfig QuickStart()
    {
        return new SessionConfig(Environment.TickCount);
    }
}