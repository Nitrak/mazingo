using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Engine
{

    public class RoomController : MonoBehaviour
    {
        private const float RoomSize = 20;

        public GameObject StartingRoom;
        public GameObject StandardRoom;

        private Maze Maze;
        private Dictionary<Location, GameObject> LoadedRooms;

        public RoomController()
        {
            var generator = new MazeGeneration();
            this.Maze = generator.GenerateNewMaze(2, 20);

            LoadedRooms = new Dictionary<Location, GameObject>();
            Load();
        }

        public void Load()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var position = player.transform.position;

            var playerTile = TryGetRoomAt(position);
            if (playerTile != null)
            {
                LoadedRooms = GetDirectionalTiles(playerTile);
            }
            else
            {
                Debug.Log("Player room not found");
            }
        }

        private Dictionary<Location, GameObject> GetDirectionalTiles(MazeTile tile)
        {
            var tiles = new Dictionary<Location, GameObject>();
            foreach (var dir in Enum.GetValues(typeof(Direction)))
            {
                var directionTile = tile;
                while ((directionTile = directionTile.Sides[(int)dir]) != null)
                {
                    
                    
                }
            }
            return tiles;
        }

        private GameObject EnsureRoom(MazeTile tile)
        {
            return null;
        }

        public MazeTile TryGetRoomAt(Vector3 vector)
        {
            var gridX = (int)(vector.x / RoomSize);
            var gridZ = (int)(vector.z / RoomSize);
            var location = new Location(gridX, gridZ);
            //if (RoomGraph.ContainsKey(location))
            //{
            //    return RoomGraph[location];
            //}
            return null;
        }

    }

}
