namespace Erg.Core.World.Generators;

public class GenerationStep
{
    public string Message { get; }
    public Area Area { get; }

    public GenerationStep(string message, Area area)
    {
        Message = message;
        Area = area;
    }
}
