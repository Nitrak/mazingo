using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Engine
{

    public class RoomController : MonoBehaviour
    {
        public const float RoomSize = 20;
        private const int StartFloorIndex = 0;

        private Maze Maze;
        private Dictionary<Location, GameObject> LoadedRooms;
        private Dictionary<string, GameObject> Prefabs;

        //State data
        private VirtualTile lastPlayerTile;

        public RoomController()
        {
            var generator = new MazeGeneration();
            this.Maze = generator.GenerateNewMaze(2, 20);
            this.lastPlayerTile = new VirtualTile(0, 0, Maze.StartTile);
            LoadedRooms = new Dictionary<Location, GameObject>();
            LoadPrefabs();
            Load();
        }

        private void LoadPrefabs()
        {
            Prefabs = new Dictionary<string, GameObject>();
            var fabs = Resources.FindObjectsOfTypeAll(typeof(GameObject));
            foreach (GameObject fab in fabs)
            {
                if (fab.tag == "RoomPrefab")
                {
                    if (!Prefabs.ContainsKey(fab.name))
                    {
                        Debug.Log(string.Format("Loaded {0}", fab.name));
                        Prefabs.Add(fab.name, fab);
                    }
                }
            }
        }

        public void Load()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var position = player.transform.position;

            var playerTile = TryGetRelativeRoom(position);
            if (playerTile != null && !lastPlayerTile.Equals(playerTile))
            {
                var newRooms = GetDirectionalTiles(playerTile);
                UnloadRoomsExcept(newRooms);
                LoadedRooms = newRooms;

                lastPlayerTile = playerTile;
            }
            else
            {
                Debug.Log("Player room not found");
            }
        }

        private void UnloadRoomsExcept(Dictionary<Location, GameObject> newRooms)
        {
            foreach (var loadedRoom in LoadedRooms)
            {
                if(!newRooms.ContainsKey(loadedRoom.Key))
                {
                    Destroy(loadedRoom.Value);
                }
            }
        }

        private Dictionary<Location, GameObject> GetDirectionalTiles(VirtualTile virtualTile)
        {
            var tiles = new Dictionary<Location, GameObject>();
            tiles.Add(virtualTile.VirtualLocation, EnsureRoom(virtualTile));
            foreach (var dir in Direction.Values)
            {
                var directionTile = virtualTile;
                while ((directionTile = directionTile.Translate(dir)) != null)
                {
                    var roomObj = EnsureRoom(directionTile);
                    tiles.Add(directionTile.VirtualLocation, roomObj);
                }
            }
            return tiles;
        }

        private GameObject EnsureRoom(VirtualTile tile)
        {
            if (LoadedRooms.ContainsKey(tile.VirtualLocation))
            {
                return LoadedRooms[tile.VirtualLocation];
            }
            var prefabName = GetPrefabName(tile);
            var prefab = Prefabs[prefabName];

            var roomPos = new Vector3(tile.GetVirtualX(), 0, tile.GetVirtualZ());
            return Instantiate(prefab, roomPos, Quaternion.identity);
        }

        private string GetPrefabName(VirtualTile virtualTile)
        {
            var builder = new StringBuilder(virtualTile.Tile.SpecialProperty == TileSpecial.PlayerSpawn ? "s" : "d");
            foreach (var sideTile in virtualTile.Tile.Sides)
            {
                builder.Append(Convert.ToInt32(sideTile != null));
            }
            return builder.ToString();
        }

        public VirtualTile TryGetRelativeRoom(Vector3 vector)
        {
            var relativeDir = GetRelativeDirection(vector);
            if(relativeDir != Direction.NONE)
            {
                return lastPlayerTile.Translate(relativeDir);
            }
            return lastPlayerTile;
        }

        private Direction GetRelativeDirection(Vector3 vector)
        {
            float relX = vector.x - lastPlayerTile.GetVirtualX();
            float relZ = vector.z - lastPlayerTile.GetVirtualZ();
            var offX = (int)Math.Floor(relX / RoomSize);
            var offZ = (int)Math.Floor(relZ / RoomSize);
            return Direction.ForRelative(offX, offZ);
        }


        private MazeTile GetTileFrom(MazeTile tile, Location location)
        {
            foreach (var dir in Enum.GetValues(typeof(Direction)))
            {
                var sideTile = tile.Sides[(int)dir];
                if (sideTile != null && sideTile.Location.Equals(location))
                {
                    return sideTile;
                }
            }
            return null;
        }
    }
}
