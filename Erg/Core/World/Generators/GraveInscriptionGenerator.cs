namespace Erg.Core.World.Generators;

public class GraveInscriptionGenerator : IStringGenerator
{
    private static readonly string[] Inscriptions =
    [
        "This grave is nameless.",
        "R.I.P.",
        "Here lies a forgotten soul.",
        "Unknown adventurer.",
        "Here lies Aldric the Bold. He delved too deep.",
        "Death is but a door. Time is but a window.",
        "The shadows claimed another.",
        "She heard the whispers. Then silence.",
        "Some doors should remain closed.",
        "The gold remains. The owner does not.",
        "Gone to explore the final dungeon.",
        "He died as he lived - running from goblins.",
        "His torch went out at the worst moment.",
        "She forgot to check for traps.",
        "Should have brought more healing potions.",
        "The treasure wasn't worth it.",
        "Do not disturb. Seriously.",
        "What lies beneath hungers still.",
        "He found what he was looking for.",
        "Three went down. None returned.",
        "The dungeon always wins."
    ];

    public string Generate(Random random) =>
        Inscriptions[random.Next(Inscriptions.Length)];
}
