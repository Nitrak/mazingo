using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Engine
{
    public class Direction
    {
        public static readonly Direction NORTH = new Direction("NORTH", 0, 1, 0);
        public static readonly Direction EAST = new Direction("EAST", 1, 0, 1);
        public static readonly Direction SOUTH = new Direction("SOUTH", 0, -1, 2);
        public static readonly Direction WEST = new Direction("WEST", -1, 0, 3);
        public static readonly Direction NONE = new Direction("NONE", 0, 0, -1);

        public static readonly Direction[] Values = new[] { NORTH, EAST, SOUTH, WEST };

        public int OffsetX { get; private set; }
        public int OffsetZ { get; private set; }
        public int Index { get; private set; }
        public string Name { get; private set; }

        private Direction(string name, int offX, int offZ, int index)
        {
            this.OffsetX = offX;
            this.OffsetZ = offZ;
            this.Index = index;
            this.Name = name;
        }
        
        public static Direction ForRelative(int x, int z)
        {
            foreach(var dir in Values)
            {
                if (dir.OffsetX == x && dir.OffsetZ == z) return dir;
            }
            return Direction.NONE;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
