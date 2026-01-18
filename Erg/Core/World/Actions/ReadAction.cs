using Erg.Core.World.Items;

namespace Erg.Core.World.Actions;

public class ReadAction : CritterAction
{
    public Book Book { get; }

    public ReadAction(Book book)
    {
        Book = book;
    }

    public override ActionResult Execute(Critter critter, Session session)
    {
        // Can't read if Reading is 0
        if (critter.Reading == 0)
        {
            if (critter == session.Player)
            {
                session.Messages.Clear();
                session.Messages.Add("You can't read.");
            }
            return Failure();
        }

        // Apply book effect
        bool consumed = Book.OnRead(critter, session);

        // Train Literacy from reading practice
        critter.TrainLiteracy(0.01, session);

        // Player messages
        if (critter == session.Player)
        {
            session.Messages.Clear();
            session.Messages.Add($"You read {Book.Name}.");

            int reading = critter.Reading;
            string comprehension = reading switch
            {
                < 20 => "You barely understood anything.",
                < 40 => "You understood some of it.",
                < 60 => "You grasped most of the content.",
                < 80 => "You understood it well.",
                _ => "You fully comprehended the text."
            };
            session.Messages.Add(comprehension);
        }

        // Remove if consumed
        if (consumed)
        {
            critter.Inventory.Remove(Book);
        }

        return Success();
    }
}
