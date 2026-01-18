using Erg.Core.Game;

namespace Erg.Core.World.Items;

/// <summary>
/// A book that trains the theoretical component of the Literacy skill when read.
/// </summary>
public class BookOfLiteracy : Book
{
    private const double BaseTraining = 0.2;

    public BookOfLiteracy(int x, int y) : base("Book of Literacy", x, y) { }

    public override bool OnRead(Critter reader, Session session)
    {
        double readingMultiplier = reader.Reading / 100.0;
        double trainingAmount = BaseTraining * readingMultiplier;
        reader.TrainLiteracyTheory(trainingAmount, session);
        return true;
    }
}
