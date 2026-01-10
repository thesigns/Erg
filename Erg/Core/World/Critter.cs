using Erg.Core.World;

public abstract class Critter : Entity
{
    public int Speed { get; protected set; }   // np. 100 = normal
    public int Energy { get; protected set; }  // akumulowana
    public Inventory Inventory { get; } = new();

    protected Critter(
        string name,
        int x,
        int y,
        char character,
        uint fg,
        uint bg,
        int speed)
        : base(name, x, y, character, fg, bg)
    {
        Speed = speed;
        Energy = 0;
    }

    public void GainEnergy()
    {
        Energy += Speed;
    }

    public bool CanAct(int cost)
    {
        return Energy >= cost;
    }

    public void SpendEnergy(int cost)
    {
        Energy -= cost;
    }
}