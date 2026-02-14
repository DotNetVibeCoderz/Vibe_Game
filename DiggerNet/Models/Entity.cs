using System;

namespace DiggerNet.Models
{
    public abstract class Entity
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;
        public Direction CurrentDirection { get; set; } = Direction.None;
        public bool IsAlive { get; set; } = true;

        public abstract string Type { get; }
    }

    public class Digger : Entity
    {
        public override string Type => "Digger";
        public bool IsDigging { get; set; }
    }

    public class Enemy : Entity
    {
        public enum EnemyType { Nobbin, Hobbin }
        public EnemyType SubType { get; set; }
        public override string Type => SubType.ToString();
        public int TransformTimer { get; set; } // Time until Nobbin transforms to Hobbin
    }

    public class Projectile : Entity
    {
        public override string Type => "Fire";
        public Direction FiredDirection { get; set; }
        public int DistanceTraveled { get; set; }
    }
}