using Erg.Core.Game;

namespace Erg.Core.World.Items;

/// <summary>
/// A book that trains the theoretical component of the Swimming skill when read.
/// </summary>
public class BookOfSwimming : Book
{
    private const double BaseTraining = 0.04;

    public BookOfSwimming(int x, int y) : base("Book of Swimming", x, y) { }

    public override bool OnRead(Critter reader, Session session)
    {
        double readingMultiplier = reader.Reading / 100.0;
        double trainingAmount = BaseTraining * readingMultiplier;
        reader.TrainSwimmingTheory(trainingAmount, session);
        return true;
    }
}
