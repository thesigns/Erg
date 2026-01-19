using Erg.Core.Game;

namespace Erg.Core.World.Items;

/// <summary>
/// A book that trains the Defense skill when read.
/// Books can only train skills up to 50%.
/// </summary>
public class BookOfDefense : Book
{
    private const double BaseTraining = 0.04;
    private const double BookCap = 0.5;

    public BookOfDefense(int x, int y) : base("Book of Defense", x, y) { }

    public override bool OnRead(Critter reader, Session session)
    {
        if (reader.Skills.DefenseSkill.CurrentValue >= BookCap)
            return true; // Book provides no benefit at this skill level

        double readingMultiplier = reader.Reading / 100.0;
        double trainingAmount = BaseTraining * readingMultiplier;
        reader.TrainDefense(trainingAmount, session);
        return true;
    }
}
