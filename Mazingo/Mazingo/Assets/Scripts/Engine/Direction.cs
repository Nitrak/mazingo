using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Engine
{
    public class Direction
    {
        public static readonly Direction NORTH = new Direction(1, 0, 0);
        public static readonly Direction EAST = new Direction(0, 1, 1);
        public static readonly Direction SOUTH = new Direction(-1, 0, 2);
        public static readonly Direction WEST = new Direction(0, -1, 3);
        public static readonly Direction NONE = new Direction(0, 0, -1);

        public static readonly Direction[] Values = new[] { NORTH, EAST, SOUTH, WEST };

        public int OffsetX { get; private set; }
        public int OffsetZ { get; private set; }
        public int Index { get; private set; }

        private Direction(int offX, int offZ, int index)
        {
            this.OffsetX = offX;
            this.OffsetZ = offZ;
            this.Index = index;
        }
        
        public static Direction ForRelative(int x, int z)
        {
            foreach(var dir in Values)
            {
                if (dir.OffsetX == x && dir.OffsetZ == z) return dir;
            }
            return Direction.NONE;
        }
    }
}
