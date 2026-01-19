using Erg.Core.Game;

namespace Erg.Core.World.Items;

/// <summary>
/// A book that trains the Attack skill when read.
/// Books can only train skills up to 50%.
/// </summary>
public class BookOfAttack : Book
{
    private const double BaseTraining = 0.04;
    private const double BookCap = 0.5;

    public BookOfAttack(int x, int y) : base("Book of Attack", x, y) { }

    public override bool OnRead(Critter reader, Session session)
    {
        if (reader.Skills.AttackSkill.CurrentValue >= BookCap)
            return true; // Book provides no benefit at this skill level

        double readingMultiplier = reader.Reading / 100.0;
        double trainingAmount = BaseTraining * readingMultiplier;
        reader.TrainAttack(trainingAmount, session);
        return true;
    }
}
