using System.Text.RegularExpressions;

namespace Erg.Core.Types;

/// <summary>
/// Represents dice notation (xDy+z) used for damage rolls and other random values.
/// </summary>
public readonly partial struct Dice
{
    /// <summary>Number of dice to roll (must be >= 1).</summary>
    public int DiceCount { get; }

    /// <summary>Number of sides on each die (must be >= 1).</summary>
    public int DiceSides { get; }

    /// <summary>Modifier added to the roll result (can be any integer).</summary>
    public int Modifier { get; }

    public Dice(int diceCount, int diceSides, int modifier = 0)
    {
        if (diceCount < 1)
            throw new ArgumentOutOfRangeException(nameof(diceCount), "Dice count must be at least 1.");
        if (diceSides < 1)
            throw new ArgumentOutOfRangeException(nameof(diceSides), "Dice sides must be at least 1.");

        DiceCount = diceCount;
        DiceSides = diceSides;
        Modifier = modifier;
    }

    /// <summary>
    /// Rolls the dice and returns the total result.
    /// </summary>
    public int Roll(Random random)
    {
        int result = Modifier;
        for (int i = 0; i < DiceCount; i++)
            result += random.Next(1, DiceSides + 1);
        return result;
    }

    /// <summary>
    /// Returns the dice notation string (e.g., "2d6+4", "1d8-2", "3d10").
    /// </summary>
    public override string ToString()
    {
        string baseNotation = $"{DiceCount}d{DiceSides}";
        return Modifier switch
        {
            > 0 => $"{baseNotation}+{Modifier}",
            < 0 => $"{baseNotation}{Modifier}",
            _ => baseNotation
        };
    }

    /// <summary>
    /// Returns the dice notation string with an extra bonus added to the modifier.
    /// Useful for displaying damage with DamageBonus included.
    /// </summary>
    public string ToString(int extraBonus)
    {
        int totalModifier = Modifier + extraBonus;
        string baseNotation = $"{DiceCount}d{DiceSides}";
        return totalModifier switch
        {
            > 0 => $"{baseNotation}+{totalModifier}",
            < 0 => $"{baseNotation}{totalModifier}",
            _ => baseNotation
        };
    }

    /// <summary>
    /// Parses a dice notation string (e.g., "2d6+4", "1d8-2", "3d10") into a Dice struct.
    /// </summary>
    public static Dice Parse(string notation)
    {
        if (!TryParse(notation, out var result))
            throw new FormatException($"Invalid dice notation: '{notation}'");
        return result;
    }

    /// <summary>
    /// Tries to parse a dice notation string into a Dice struct.
    /// </summary>
    public static bool TryParse(string notation, out Dice result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(notation))
            return false;

        var match = DiceNotationRegex().Match(notation.Trim());
        if (!match.Success)
            return false;

        if (!int.TryParse(match.Groups[1].Value, out int diceCount) || diceCount < 1)
            return false;

        if (!int.TryParse(match.Groups[2].Value, out int diceSides) || diceSides < 1)
            return false;

        int modifier = 0;
        if (match.Groups[3].Success && !string.IsNullOrEmpty(match.Groups[3].Value))
        {
            if (!int.TryParse(match.Groups[3].Value, out modifier))
                return false;
        }

        result = new Dice(diceCount, diceSides, modifier);
        return true;
    }

    /// <summary>
    /// Implicit conversion from int to Dice for convenience.
    /// Creates a Dice that always returns the specified value (1d1 + (value-1)).
    /// </summary>
    public static implicit operator Dice(int value) => new(1, 1, value - 1);

    [GeneratedRegex(@"^(\d+)[dD](\d+)([+-]\d+)?$")]
    private static partial Regex DiceNotationRegex();
}
