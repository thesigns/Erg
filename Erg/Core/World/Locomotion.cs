namespace Erg.Core.World;

public enum Locomotion
{
    Terrestrial,  // Land only
    Amphibious,   // Land + water, prefers land
    Semiaquatic,  // Water + land, prefers water
    Aquatic,      // Water only
    Aerial        // Flying
}
