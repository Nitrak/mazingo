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

        public Rigidbody StartingRoom;
        public Rigidbody StandardRoom;

        private Maze Maze;
        private IEnumerable<MazeTile> LoadedTiles;

        public RoomController()
        {
            var generator = new MazeGeneration();
            this.Maze = generator.GenerateNewMaze(2, 20);

            LoadedTiles = new List<MazeTile>();
            Load();
        }

        public void Load()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var position = player.transform.position;

            var playerTile = TryGetRoomAt(position);
            if (playerTile != null)
            {
                LoadedTiles = GetDirectionalTiles(playerTile);
            }
            else
            {
                Debug.Log("Player room not found");
            }
        }

        private IEnumerable<MazeTile> GetDirectionalTiles(MazeTile tile)
        {
            var tiles = new List<MazeTile>();
            foreach (var dir in Enum.GetValues(typeof(Direction)))
            {
                var directionTile = tile;
                while ((directionTile = directionTile.Sides[(int)dir]) != null)
                {
                    tiles.Add(directionTile);
                }
            }
            return tiles;
        }

        public MazeTile TryGetRoomAt(Vector3 vector)
        {
            var gridX = (int)(vector.x / RoomSize);
            var gridZ = (int)(vector.z / RoomSize);
            var location = new Location(gridX, gridZ);
            if (RoomGraph.ContainsKey(location))
            {
                return RoomGraph[location];
            }
            return null;
        }

    }

}
