namespace Erg.Core.World.Generators;

public interface IDungeonGenerator
{
    Area Generate();
    (int x, int y) GetPlayerStartPosition();
}
