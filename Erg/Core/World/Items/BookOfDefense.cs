using Erg.Core.Game;

namespace Erg.Core.World.Items;

/// <summary>
/// A book that trains the Defense skill when read.
/// </summary>
public class BookOfDefense : Book
{
    private const double BaseTraining = 0.2;

    public BookOfDefense(int x, int y) : base("Book of Defense", x, y) { }

    public override bool OnRead(Critter reader, Session session)
    {
        double readingMultiplier = reader.Reading / 100.0;
        double trainingAmount = BaseTraining * readingMultiplier;
        reader.TrainDefense(trainingAmount, session);
        return true;
    }
}
