using Erg.Core.Game;

namespace Erg.Core.World.Items;

/// <summary>
/// A book that trains the Attack skill when read.
/// </summary>
public class BookOfAttack : Book
{
    private const double BaseTraining = 0.2;

    public BookOfAttack(int x, int y) : base("Book of Attack", x, y) { }

    public override bool OnRead(Critter reader, Session session)
    {
        double readingMultiplier = reader.Reading / 100.0;
        double trainingAmount = BaseTraining * readingMultiplier;
        reader.TrainAttack(trainingAmount, session);
        return true;
    }
}
