using Erg.Core.Game;

namespace Erg.Core.World.Items;

/// <summary>
/// A book that trains the theoretical component of the Unarmed skill when read.
/// </summary>
public class BookOfUnarmed : Book
{
    private const double BaseTraining = 0.2;

    public BookOfUnarmed(int x, int y) : base("Book of Unarmed", x, y) { }

    public override bool OnRead(Critter reader, Session session)
    {
        double readingMultiplier = reader.Reading / 100.0;
        double trainingAmount = BaseTraining * readingMultiplier;
        reader.TrainUnarmedTheory(trainingAmount, session);
        return true;
    }
}
