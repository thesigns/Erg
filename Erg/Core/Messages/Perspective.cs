using Erg.Core.World;

namespace Erg.Core.Messages;

public static class Perspective
{
    public static string NameOf(Critter c, bool subject = true) =>
        c is Player ? (subject ? "You" : "you") : c.Name;

    public static string Verb(Critter c, string youForm, string thirdForm) =>
        c is Player ? youForm : thirdForm;
}
