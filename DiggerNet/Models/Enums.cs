namespace DiggerNet.Models
{
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public enum TileType
    {
        Empty,
        Dirt,
        Emerald,
        GoldBag,
        Wall,
        OpenGoldBag // When gold bag breaks
    }
}