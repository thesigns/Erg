namespace Erg.Core.Types;

public record PronounSet(string Subject, string Object, string Possessive)
{
    public static readonly PronounSet He = new("He", "him", "his");
    public static readonly PronounSet She = new("She", "her", "her");
    public static readonly PronounSet It = new("It", "it", "its");
}
