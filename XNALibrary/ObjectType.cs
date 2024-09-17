namespace XNALibrary;

[Flags]
public enum ObjectType
{
    Particle = 1,
    Player = 2,
    Enemy = 4,
    Tile = 8,
    ActiveObject = 16,
    Pickup = 32
}