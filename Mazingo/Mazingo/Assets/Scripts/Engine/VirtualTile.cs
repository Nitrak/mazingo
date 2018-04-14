using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Engine
{
    public class VirtualTile
    {

        public Location VirtualLocation { get; private set; }
        public MazeTile Tile { get; private set; }
        public GameObject GameObject { get; set; }

        public VirtualTile(int x, int z, MazeTile tile)
        {
            this.VirtualLocation = new Location(x, z);
            this.Tile = tile;
        }

        public int GetVirtualX()
        {
            return (int)(VirtualLocation.X * RoomController.RoomSize);
        }

        public int GetVirtualZ()
        {
            return (int)(VirtualLocation.Z * RoomController.RoomSize);
        }

        public VirtualTile Translate(Direction direction)
        {
            var nextTile = Tile.Sides[direction.Index];
            if (nextTile != null) { 
                return new VirtualTile(VirtualLocation.X + direction.OffsetX, VirtualLocation.Z + direction.OffsetZ, nextTile);
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            var tileObj = obj as VirtualTile;
            if(tileObj != null)
            {
                return tileObj.VirtualLocation.Equals(this.VirtualLocation);
            }
            return false;
        }
    }
}
