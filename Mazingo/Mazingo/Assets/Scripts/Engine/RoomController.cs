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
        public static readonly Vector3 GridOffset = new Vector3(10, 0, 10);
        public static readonly Vector3 PlayerSpawnOffset = new Vector3(0, 1, 0);
        private const int StartFloorIndex = 0;

        private Maze Maze;
        private Dictionary<Location, KeyValuePair<GameObject, GameObject>> LoadedRooms;
        private Dictionary<string, GameObject> Prefabs;
        private MazeGeneration generator;
        private GameObject player;
        private PlayerController playerController;

        //State data
        private VirtualTile lastPlayerTile;

        public RoomController()
        {
            this.player = GameObject.FindGameObjectWithTag("Player");
            this.generator = new MazeGeneration();
            this.playerController = player.transform.GetChild(0).GetComponent<PlayerController>();
            LoadedRooms = new Dictionary<Location, KeyValuePair<GameObject, GameObject>>();
            LoadPrefabs();
            //Load();

            playerController.OnDeath += ReloadStartRoom;
        }

        public bool IsLoaded()
        {
            return Maze != null;
        }

        public void ReloadStartRoom(object sender)
        {
            var newVirtual = new VirtualTile(0, 0, Maze.StartTile);
            Load(newVirtual);
        }

        public void StartTutorial()
        {
            this.Maze = generator.GenerateTutorialMaze();
            this.lastPlayerTile = new VirtualTile(0, 0, Maze.StartTile);
            RespawnHack();
        }

        public void StartLevel(int level)
        {
            this.Maze = generator.GenerateNewMaze(new[] { 10, 10, 10 }, .30d);
            this.lastPlayerTile = new VirtualTile(0, 0, Maze.StartTile);
            RespawnHack();
        }


        private void RespawnHack()
        {
            //player.transform.position = GetSpawnPosition(lastPlayerTile);
            playerController.SetSpawnPosition(GetSpawnPosition(lastPlayerTile));
            playerController.Kill();
        }

        private Vector3 GetSpawnPosition(VirtualTile virtualTile)
        {
            var prefabName = GetPrefabName(virtualTile);
            var prefab = Prefabs[prefabName];
            return GridOffset + prefab.transform.position + PlayerSpawnOffset;
        }

        private void LoadPrefabs()
        {
            Prefabs = new Dictionary<string, GameObject>();
            var fabs = Resources.FindObjectsOfTypeAll(typeof(GameObject));
            foreach (GameObject fab in fabs)
            {
                if (fab.tag == "Prefab")
                {
                    if (!Prefabs.ContainsKey(fab.name.ToLower()))
                    {
                        Debug.Log(string.Format("Loaded {0}", fab.name));
                        Prefabs.Add(fab.name.ToLower(), fab);
                    }
                }
            }
        }

        public void Load(VirtualTile loadAt = null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var position = player.transform.position;
            //Debug.Log(string.Format("Player position: {0}", position));

            var playerTile = loadAt ?? TryGetRelativeRoom(position);
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

        private void UnloadRoomsExcept(Dictionary<Location, KeyValuePair<GameObject, GameObject>> newRooms)
        {
            foreach (var loadedRoom in LoadedRooms)
            {
                if (!newRooms.ContainsKey(loadedRoom.Key))
                {
                    var objKvp = loadedRoom.Value;

                    Destroy(objKvp.Key);
                    Destroy(objKvp.Value);
                }
            }
        }

        private Dictionary<Location, KeyValuePair<GameObject, GameObject>> GetDirectionalTiles(VirtualTile virtualTile)
        {
            var tiles = new Dictionary<Location, KeyValuePair<GameObject, GameObject>>();
            tiles.Add(virtualTile.VirtualLocation, EnsureRoom(virtualTile));
            foreach (var dir in Direction.Values)
            {
                var directionTile = virtualTile;
                var loaded = 0;
                while (loaded <= 5 && (directionTile = directionTile.Translate(dir)) != null)
                {
                    var roomObj = EnsureRoom(directionTile);
                    //Debug.Log(string.Format("Loading room {0} of {1} ({2} at {3})", dir.Name, virtualTile.VirtualLocation, roomObj.name, directionTile.VirtualLocation));
                    tiles.Add(directionTile.VirtualLocation, roomObj);
                    loaded++;
                }
            }
            return tiles;
        }

        private KeyValuePair<GameObject, GameObject> EnsureRoom(VirtualTile tile)
        {
            if (LoadedRooms.ContainsKey(tile.VirtualLocation))
            {
                return LoadedRooms[tile.VirtualLocation];
            }
            var prefabName = GetPrefabName(tile);
            Debug.Log("Getting prefab: " + prefabName);
            var prefab = Prefabs[prefabName];

            var roomPos = new Vector3(tile.GetVirtualX(), 0, tile.GetVirtualZ());
            var roomObject = Instantiate(prefab, roomPos + prefab.transform.position + GridOffset, prefab.transform.rotation);

            var keyPrefab = GetRoomKeyContent(tile.Tile);
            GameObject keyInstance = null;
            if (keyPrefab != null)
            {
                keyInstance = Instantiate(keyPrefab, roomPos + prefab.transform.position + keyPrefab.transform.position + GridOffset, keyPrefab.transform.rotation);
                keyInstance.SetActive(true);
            }

            roomObject.SetActive(true);
            var destructible = (tile.Tile.SpecialProperty != TileSpecial.Key1) ? keyInstance : null;
            //roomObject.transform.position = prefab.transform.position;
            return new KeyValuePair<GameObject, GameObject>(roomObject, destructible);
        }

        private GameObject GetRoomKeyContent(MazeTile mazeTile)
        {
            if (mazeTile.SpecialProperty == TileSpecial.Nothing) return null;

            var prefabName = string.Empty;
            switch (mazeTile.SpecialProperty)
            {
                case TileSpecial.BreakingPoint1:
                    prefabName = "keydoor";
                    break;
                case TileSpecial.Key1:
                    if (!playerController.HasBomb())
                    {
                        prefabName = "boomzingo";
                    }
                    break;
                default:
                    prefabName = Enum.GetName(typeof(TileSpecial), mazeTile.SpecialProperty).ToLower();
                    break;
            }
            if (!string.IsNullOrEmpty(prefabName) && Prefabs.ContainsKey(prefabName))
            {
                Debug.Log(string.Format("Loading prefab: {0}", prefabName));
                return Prefabs[prefabName];
            }
            return null;
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
            if (relativeDir != Direction.NONE)
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
