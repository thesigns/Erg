using Erg.Core.Abstractions;

namespace Erg.Core.Game;

public class CheatKeys
{
    public void Update(IInput input, Session? session)
    {
        if (session == null) return;

        // F5: Toggle reveal all map
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.F5))
        {
            session.Fov.RevealAll = !session.Fov.RevealAll;

            if (session.Fov.RevealAll)
            {
                // Mark all tiles as explored (permanent)
                for (int y = 0; y < session.Area.Height; y++)
                    for (int x = 0; x < session.Area.Width; x++)
                        session.Area.MarkExplored(x, y);
            }
        }
    }
}
