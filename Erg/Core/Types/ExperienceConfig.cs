using System;

namespace Erg.Core.Types;

public static class ExperienceConfig
{
    public const int BaseXP = 100;
    public const float Exponent = 2.0f;

    public static int CalculateXPForLevel(int level)
    {
        // level 1→2: 100, level 2→3: 400, level 3→4: 900, etc.
        return (int)(BaseXP * Math.Pow(level, Exponent));
    }
}
