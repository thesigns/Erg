using System;

namespace Erg.Core.Types;

public static class ExperienceConfig
{
    public const int BaseXP = 100;
    public const float Exponent = 2.0f;

    public static int CalculateXPForLevel(int level)
    {
        return (int)(BaseXP * Math.Pow(level + 1, Exponent));
    }
}
