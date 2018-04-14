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
        private const float RoomSize = 20;
        private const int StartFloorIndex = 0;

        private Maze Maze;
        private Dictionary<Location, KeyValuePair<int, GameObject>> LoadedRooms;

        //State data
        private MazeTile lastPlayerTile;

        public RoomController()
        {
            var generator = new MazeGeneration();
            this.Maze = generator.GenerateNewMaze(2, 20);

            LoadedRooms = new Dictionary<Location, KeyValuePair<int, GameObject>>();
            Load();
        }

        public void Load()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var position = player.transform.position;

            var playerTile = TryGetRoomAt(position);
            if (playerTile != null)
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

        private void UnloadRoomsExcept(Dictionary<Location, KeyValuePair<int, GameObject>> newRooms)
        {
            foreach(var loadedRoomKvp in LoadedRooms)
            {
                var location = loadedRoomKvp.Key;
                var newRoom = newRooms[location];
                var oldRoom = loadedRoomKvp.Value;
                if(newRooms.ContainsKey(location) && newRoom.Key == oldRoom.Key)
                {
                    continue;
                }
                Destroy(oldRoom.Value);
            }
        }

        private Dictionary<Location, KeyValuePair<int, GameObject>> GetDirectionalTiles(MazeTile tile)
        {
            var tiles = new Dictionary<Location, KeyValuePair<int, GameObject>>();
            foreach (var dir in Enum.GetValues(typeof(Direction)))
            {
                var directionTile = tile;
                while ((directionTile = directionTile.Sides[(int)dir]) != null)
                {
                    var roomKvp = new KeyValuePair<int, GameObject>(directionTile.Floor, EnsureRoom(directionTile));
                    LoadedRooms.Add(directionTile.Location, roomKvp);
                }
            }
            return tiles;
        }

        private GameObject EnsureRoom(MazeTile tile)
        {
            if(LoadedRooms.ContainsKey(tile.Location) && LoadedRooms[tile.Location].Key == tile.Floor)
            {
                return LoadedRooms[tile.Location].Value;
            }
            var prefabName = GetPrefabName(tile);
            var prefab = GameObject.Find(prefabName);
            Resources.FindObjectsOfTypeAll(typeof(GameObject));
            var roomPos = new Vector3(tile.Location.X * RoomSize, 0, tile.Location.Z * RoomSize);
            return Instantiate(prefab, roomPos, Quaternion.identity);
        }

        private string GetPrefabName(MazeTile tile)
        {
            var builder = new StringBuilder(tile.SpecialProperty == TileSpecial.PlayerSpawn ? "s" : "d");
            foreach(var sideTile in tile.Sides) {
                builder.Append(Convert.ToInt32(sideTile != null));
            }
            return builder.ToString();
        }

        public MazeTile TryGetRoomAt(Vector3 vector)
        {
            var gridX = (int)(vector.x / RoomSize);
            var gridZ = (int)(vector.z / RoomSize);
            var location = new Location(gridX, gridZ);
            if (lastPlayerTile == null)
            {
                Floor floor = Maze.Floors[StartFloorIndex];
                if (floor.Tiles.ContainsKey(location))
                {
                    return floor.Tiles[location];
                }
            }
            else
            {
                if (lastPlayerTile.Location.Equals(location))
                {
                    return lastPlayerTile;
                }
                else
                {
                    return GetTileFrom(lastPlayerTile, location);
                }
            }
            return null;
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
