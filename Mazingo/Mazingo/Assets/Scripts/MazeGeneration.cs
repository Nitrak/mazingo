using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts
{
    public enum TileSpecial
    {
        Nothing = 0,
        PlayerSpawn = 4001,
        StrangerDanger = 2,
        LavaPuzzle = 3,
        LavaPuzzleWithDanger = 4,

        Key1 = 1001,


        BreakingPoint1 = 2001,

        Decoration1 = 3001,
        Decoration2 = 3002,
        Decoration3 = 3003,
        Decoration4 = 3004,
        Decoration5 = 3005,
        Decoration6 = 3006,
        Decoration7 = 3007
    }

    public enum Direction
    {
        North = 0,
        South = 1,
        West = 2,
        East = 3
    }

    public class MazeTile
    {
        //Used for the first tile of a floor. Subsequent calls must be the second constructor
        public MazeTile(TileSpecial special, int floor, ref Maze maze)
        {
            maze.Floors[floor] = maze.Floors[floor] == null ? new Floor() : maze.Floors[floor];
            SpecialProperty = special;
            Location = new Location(0, 0);
            Floor = floor;
            
            maze.Floors[floor].Tiles.Add(Location, this);
        }

        public MazeTile(TileSpecial special, Direction growDirection, ref MazeTile tileBackwards, ref Maze maze)
        {
            if (tileBackwards == null)
            {
                throw new ArgumentNullException("tileBackwards");
            }
            maze.Floors[tileBackwards.Floor] = maze.Floors[tileBackwards.Floor] == null ? new Floor() : maze.Floors[tileBackwards.Floor];
            Location newLocation;

            switch (growDirection)
            {
                case Direction.East:
                    newLocation = new Location(tileBackwards.Location.X + 1, tileBackwards.Location.Z);
                    West = tileBackwards;
                    Location = newLocation;
                    tileBackwards.East = this;
                    break;
                case Direction.North:
                    newLocation = new Location(tileBackwards.Location.X, tileBackwards.Location.Z + 1);
                    South = tileBackwards;
                    Location = newLocation;
                    tileBackwards.North = this;
                    break;
                case Direction.South:
                    newLocation = new Location(tileBackwards.Location.X, tileBackwards.Location.Z - 1);
                    North = tileBackwards;
                    Location = newLocation;
                    tileBackwards.South = this;
                    break;
                case Direction.West:
                    newLocation = new Location(tileBackwards.Location.X - 1, tileBackwards.Location.Z);
                    East = tileBackwards;
                    Location = newLocation;
                    tileBackwards.West = this;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("growDirection", growDirection, null);
            }

            Location = newLocation;
            SpecialProperty = special;
            Floor = tileBackwards.Floor;
            maze.Floors[Floor].Tiles.Add(Location, this);
        }

        public TileSpecial SpecialProperty;
        public Location Location;
        public int Floor;

        //null is a wall, otherwise contains (a pointer to) the tile it leads to
        public MazeTile North;
        public MazeTile South;
        public MazeTile West;
        public MazeTile East;

        private MazeTile[] _sides;
        public MazeTile[] Sides
        {
            get
            {
                return _sides ?? (_sides = new[] { North, East, South, West });
            }
        }

        public MazeTile SideByDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.East:
                    return East;
                case Direction.North:
                    return North;
                case Direction.South:
                    return South;
                case Direction.West:
                    return West;
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, "Directions must be one of four cardinal directions, as given in the enumerable");
            }
        }

        public void TieMazeTile(MazeTile other, Direction directionToOther)
        {
            switch (directionToOther)
            {
                case Direction.North:
                    North = other;
                    other.South = this;
                    break;
                case Direction.South:
                    South = other;
                    other.North = this;
                    break;
                case Direction.West:
                    West = other;
                    other.East = this;
                    break;
                case Direction.East:
                    East = other;
                    other.West = this;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("directionToOther", directionToOther, null);
            }
        }

        public override bool Equals(object obj)
        {
            var tileObj = obj as MazeTile;
            if(tileObj != null)
            {
                return tileObj.Floor == this.Floor
                    && tileObj.Location.Equals(this.Location);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return this.Floor.GetHashCode() + this.Location.GetHashCode();
        }
    }

    public struct Location
    {
        public readonly int X;
        public readonly int Z;

        public Location(int x, int z)
        {
            X = x;
            Z = z;
        }

        public override int GetHashCode()
        {
            var hash = 17;
            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Z.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Z);
        }

        public Location AddDirectionOnce(Direction direction)
        {
            switch (direction)
            {
                case Direction.East:
                    return new Location(X+1,Z);
                case Direction.North:
                    return new Location(X,Z+1);
                case Direction.South:
                    return new Location(X,Z-1);
                case Direction.West:
                    return new Location(X-1,Z);;
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, "Directions must be one of four cardinal directions, as given in the enumerable");
            }
        }
    }

    public class Floor
    {
        public Dictionary<Location, MazeTile> Tiles = new Dictionary<Location, MazeTile>();
        public int NumPortsToOtherFloors;
    }

    public class Maze
    {
        public List<Floor> Floors = new List<Floor>();
        public MazeTile StartTile;
        public int ShortestTilesFromKeyToBomb;
    }

    public static class HelperFunctions
    {
        
    }

    public class MazeGeneration : MonoBehaviour
    {
        public int FindDistance(ref Maze maze)
        {
            var listOfAllTiles = maze.Floors.SelectMany(e => e.Tiles.Select(a => a.Value)).ToList();
            Dictionary<MazeTile, int> objToIndex = new Dictionary<MazeTile, int>();
            int origin = -1; 
            int target = -1;

            var distance = new int[listOfAllTiles.Count()];
            var previous = new int[listOfAllTiles.Count()];
            var visited = new bool[listOfAllTiles.Count()];


            for (int i = 0; i < listOfAllTiles.Count; i++)
            {
                var m =  listOfAllTiles[i];
                if (m.SpecialProperty == TileSpecial.Key1)
                    target = i;
                else if (m.SpecialProperty == TileSpecial.BreakingPoint1)
                    origin = i;

                objToIndex.Add(m, i);
                distance[i] = int.MaxValue;
                previous[i] = -1;
                visited[i] = false;
            }

            distance[origin] = 0;

            while (listOfAllTiles.Count > 0)
            {
                var uIdx = GetShortest(distance, visited);
                if (uIdx < 0)
                    break;
                var u = listOfAllTiles[uIdx];

                if (u == null)
                    break;
                visited[uIdx] = true;

                if (uIdx == target)
                {
                    Debug.Log(distance[uIdx]);
                    return distance[uIdx];
                }

                foreach(var v in u.Sides)
                {
                    if (v == null) continue;
                    var vIdx = objToIndex[v];
                    var alt = distance[uIdx] + 1;
                    if(alt < distance[vIdx])
                    {
                        distance[vIdx] = alt;
                        previous[vIdx] = uIdx;
                    }
                }

            }
            Debug.Log("No path!");
            return int.MaxValue;
        }

        private int GetShortest(int[] distance, bool[] visited)
        {
            int shortestIdx = -1;
            int shortestDist = int.MaxValue;

            for (int i = 0; i < distance.Length; i++) {
                if (visited[i]) continue;

                int currentDist = distance[i];
                if (currentDist < shortestDist)
                    shortestIdx = i;
            }
            return shortestIdx;
        }

        public Maze GenerateTutorialMaze()
        {
            var maze = new Maze();
            maze.Floors.AddRange(Enumerable.Repeat<Floor>(null, 2).ToList());
            
            //Create ground floor
            var starting = new MazeTile(TileSpecial.PlayerSpawn, 0, ref maze);
            var room1 = new MazeTile(TileSpecial.Decoration1, Direction.East, ref starting, ref maze);
            var room2 = new MazeTile(TileSpecial.Decoration3, Direction.North, ref room1, ref maze);
            var room3 = new MazeTile(TileSpecial.Nothing, Direction.East, ref room2, ref maze);
            var bproom = new MazeTile(TileSpecial.BreakingPoint1, Direction.East, ref room3, ref maze);
            var statue = new MazeTile(TileSpecial.StrangerDanger, Direction.North, ref room3, ref maze);
            var bomb = new MazeTile(TileSpecial.Decoration6, Direction.North, ref statue, ref maze);
            
            //Create 1st floor
            var floor1Room1 = new MazeTile(TileSpecial.Decoration2, 1, ref maze);
            var floor1Room2 = new MazeTile(TileSpecial.Key1, Direction.South, ref floor1Room1, ref maze);
            var floor1Lava = new MazeTile(TileSpecial.LavaPuzzle, Direction.East, ref floor1Room2, ref maze);
            var floor1Room3 = new MazeTile(TileSpecial.Nothing, Direction.East, ref floor1Lava, ref maze);

            //Hooking the two floors together
            
            maze.StartTile = starting;
            statue.TieMazeTile(floor1Room1, Direction.East);
            floor1Room3.TieMazeTile(bomb, Direction.East);

            maze.ShortestTilesFromKeyToBomb = FindDistance(ref maze);

            return maze;
        }

        private TileSpecial GenerateRandomTrap(double pctchance, ref Random rng)
        {
            //Traps are found in the enum from 1 to 1000
            var rngNum = rng.NextDouble();
            if (!(rngNum <= pctchance)) return TileSpecial.Nothing;

            var possibleTraps = Enum.GetValues(typeof(TileSpecial)).Cast<int>().Where(e => e > 0 && e < 1000).ToList();
            return (TileSpecial) possibleTraps[rng.Next(0, possibleTraps.Count - 1)];
        }

        private TileSpecial GenerateRandomDecoration(ref Random rng)
        {
            //Traps are found in the enum from 1 to 1000
            if (!(rng.NextDouble() <= 0.5)) return TileSpecial.Nothing;

            var possibleTraps = Enum.GetValues(typeof(TileSpecial)).Cast<int>().Where(e => e > 3000 && e < 4000).ToList();
            return (TileSpecial) possibleTraps[rng.Next(0, possibleTraps.Count - 1)];
        }
        
        public Maze GenerateNewMaze(int[] tilesPerFloor, double chanceOfTrap)
        {
            //Get optional params
            var Floors = tilesPerFloor.Length;

            //Set up the maze
            var rng = new Random();
            var seed = rng.Next(int.MaxValue);
            Debug.Log(seed);
            rng = new Random(seed);
            var maze = new Maze();
            

            maze.Floors.AddRange(Enumerable.Repeat<Floor>(null, Floors).ToList());

            for (int floor = 0; floor < Floors; ++floor)
            {

                var previousTile = new MazeTile(GenerateRandomTrap(chanceOfTrap, ref rng), floor, ref maze);


                for (int tile = 0; tile < tilesPerFloor[floor]; ++tile)
                {
                    var nextDirection = (Direction) rng.Next(0, 3);

                    MazeTile newTile;
                    if (nextDirection == Direction.East && maze.Floors[floor].Tiles
                            .TryGetValue(new Location(previousTile.Location.X + 1, previousTile.Location.Z),
                                out newTile))
                    {
                        previousTile.TieMazeTile(newTile, nextDirection);
                    }
                    else if (nextDirection == Direction.West && maze.Floors[floor].Tiles
                                 .TryGetValue(new Location(previousTile.Location.X - 1, previousTile.Location.Z),
                                     out newTile))
                    {
                        previousTile.TieMazeTile(newTile, nextDirection);
                    }
                    else if (nextDirection == Direction.North && maze.Floors[floor].Tiles
                                 .TryGetValue(new Location(previousTile.Location.X, previousTile.Location.Z + 1),
                                     out newTile))
                    {
                        previousTile.TieMazeTile(newTile, nextDirection);
                    }
                    else if (nextDirection == Direction.South && maze.Floors[floor].Tiles
                                 .TryGetValue(new Location(previousTile.Location.X, previousTile.Location.Z - 1),
                                     out newTile))
                    {
                        previousTile.TieMazeTile(newTile, nextDirection);
                    }
                    else
                    {
                        newTile = new MazeTile(GenerateRandomTrap(chanceOfTrap, ref rng), nextDirection, ref previousTile, ref maze);
                    }

                    previousTile = newTile;
                }
            }

            //Now we have completely generated the mazes. Let's glue them together
            for (int floor = 1; floor < Floors; ++floor)
            {
                for (int otherFloor = 0; otherFloor < Floors; ++otherFloor)
                {
                    //Don't make floors back to the same one
                    if (floor == otherFloor)
                        continue;

                    var possibleDoorsOnThisFloorEast =
                        maze.Floors[floor].Tiles.Where(e => e.Value.East == null).ToList();
                    var possibleDoorsOnThisFloorWest =
                        maze.Floors[floor].Tiles.Where(e => e.Value.West == null).ToList();
                    var possibleDoorsOnThisFloorNorth =
                        maze.Floors[floor].Tiles.Where(e => e.Value.North == null).ToList();
                    var possibleDoorsOnThisFloorSouth =
                        maze.Floors[floor].Tiles.Where(e => e.Value.South == null).ToList();

                    var possibleDoorsOnOtherFloorEast =
                        maze.Floors[otherFloor].Tiles.Where(e => e.Value.East == null).ToList();
                    var possibleDoorsOnOtherFloorWest =
                        maze.Floors[otherFloor].Tiles.Where(e => e.Value.West == null).ToList();
                    var possibleDoorsOnOtherFloorNorth =
                        maze.Floors[otherFloor].Tiles.Where(e => e.Value.North == null).ToList();
                    var possibleDoorsOnOtherFloorSouth =
                        maze.Floors[otherFloor].Tiles.Where(e => e.Value.South == null).ToList();

                    int toOtherFloors = maze.Floors[floor].NumPortsToOtherFloors == 0 ? rng.Next(1,Math.Max((int) Math.Floor(tilesPerFloor[floor]/(10.0*Floors)),1)) : rng.Next(0, (int) Math.Floor(tilesPerFloor[floor]/(10.0*Floors)));

                    for (int portal = 0; portal < toOtherFloors; ++portal)
                    {
                        maze.Floors[floor].NumPortsToOtherFloors++;
                        maze.Floors[otherFloor].NumPortsToOtherFloors++;
                        var randomDirectionFromThisFloor = (Direction) rng.Next(0, 3);
                        if (randomDirectionFromThisFloor == Direction.East
                            && possibleDoorsOnThisFloorEast.Count > 0
                            && possibleDoorsOnOtherFloorWest.Count > 0)
                        {
                            var idxFromThisFloor = rng.Next(0, possibleDoorsOnThisFloorEast.Count - 1);
                            var idxFromOtherFloor = rng.Next(0, possibleDoorsOnOtherFloorWest.Count - 1);
                            possibleDoorsOnThisFloorEast[idxFromThisFloor].Value.TieMazeTile(possibleDoorsOnOtherFloorWest[idxFromOtherFloor].Value, randomDirectionFromThisFloor);
                            possibleDoorsOnThisFloorEast.RemoveAt(idxFromThisFloor);
                            possibleDoorsOnOtherFloorWest.RemoveAt(idxFromOtherFloor);
                        }
                        else if (randomDirectionFromThisFloor == Direction.West
                                 && possibleDoorsOnThisFloorWest.Count > 0
                                 && possibleDoorsOnOtherFloorEast.Count > 0)
                        {
                            var idxFromThisFloor = rng.Next(0, possibleDoorsOnThisFloorWest.Count - 1);
                            var idxFromOtherFloor = rng.Next(0, possibleDoorsOnOtherFloorEast.Count - 1);
                            possibleDoorsOnThisFloorWest[idxFromThisFloor].Value.TieMazeTile(possibleDoorsOnOtherFloorEast[idxFromOtherFloor].Value, randomDirectionFromThisFloor);
                            possibleDoorsOnThisFloorWest.RemoveAt(idxFromThisFloor);
                            possibleDoorsOnOtherFloorEast.RemoveAt(idxFromOtherFloor);
                        }
                        else if (randomDirectionFromThisFloor == Direction.North
                                 && possibleDoorsOnThisFloorNorth.Count > 0
                                 && possibleDoorsOnOtherFloorSouth.Count > 0)
                        {
                            var idxFromThisFloor = rng.Next(0, possibleDoorsOnThisFloorNorth.Count - 1);
                            var idxFromOtherFloor = rng.Next(0, possibleDoorsOnOtherFloorSouth.Count - 1);
                            possibleDoorsOnThisFloorNorth[idxFromThisFloor].Value.TieMazeTile(possibleDoorsOnOtherFloorSouth[idxFromOtherFloor].Value, randomDirectionFromThisFloor);
                            possibleDoorsOnThisFloorNorth.RemoveAt(idxFromThisFloor);
                            possibleDoorsOnOtherFloorSouth.RemoveAt(idxFromOtherFloor);
                        }
                        else if (randomDirectionFromThisFloor == Direction.South
                                 && possibleDoorsOnThisFloorSouth.Count > 0
                                 && possibleDoorsOnOtherFloorNorth.Count > 0)
                        {
                            var idxFromThisFloor = rng.Next(0, possibleDoorsOnThisFloorSouth.Count - 1);
                            var idxFromOtherFloor = rng.Next(0, possibleDoorsOnOtherFloorNorth.Count - 1);
                            possibleDoorsOnThisFloorSouth[idxFromThisFloor].Value.TieMazeTile(possibleDoorsOnOtherFloorNorth[idxFromOtherFloor].Value, randomDirectionFromThisFloor);
                            possibleDoorsOnThisFloorSouth.RemoveAt(idxFromThisFloor);
                            possibleDoorsOnOtherFloorNorth.RemoveAt(idxFromOtherFloor);
                        }

                    }
                }
            }

            //Insert key and breaking point
            var allTiles = maze.Floors.SelectMany(e => e.Tiles.Select(a => a.Value)).ToList();

            var tileIndex = rng.Next(allTiles.Count - 1);
            allTiles[tileIndex].SpecialProperty = TileSpecial.Key1;
            allTiles.RemoveAt(tileIndex);

            tileIndex = rng.Next(allTiles.Count - 1);
            allTiles[tileIndex].SpecialProperty = TileSpecial.BreakingPoint1;
            allTiles.RemoveAt(tileIndex);

            //Add a bunch of decorations
            foreach (var mazeTile in allTiles)
            {
                if(mazeTile.SpecialProperty == TileSpecial.Nothing)
                    mazeTile.SpecialProperty = GenerateRandomDecoration(ref rng);
            }

            //Add a start position to a southern wall somewhere
            for (var i = 0; i < 3 /*Number of directions*/; ++i)
            {
                var usingDirection = (Direction)i;
                var listOfValidStartTiles = maze.Floors.SelectMany(e => e.Tiles.Select(a => a.Value))
                    .Where(e => e.SideByDirection(usingDirection) == null 
                            && !maze.Floors[e.Floor].Tiles.ContainsKey(e.Location.AddDirectionOnce(usingDirection))).ToList();
                if (listOfValidStartTiles.Count == 0) continue;
                var aValidTile = listOfValidStartTiles[rng.Next(listOfValidStartTiles.Count-1)];

                maze.StartTile = new MazeTile(TileSpecial.PlayerSpawn, usingDirection, ref aValidTile, ref maze);
                break;
            }

            maze.ShortestTilesFromKeyToBomb = FindDistance(ref maze);

            return maze;
        }

        // Use this for initialization
        // ReSharper disable once UnusedMember.Local
        void Start()
        {
            var attempt = 5;
            while (attempt > 0)
            {
                try
                {
                    var maze = GenerateNewMaze(new []{30}, 0.2);
                    break;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    attempt--;
                }

            }
            

            
        }

        // Update is called once per frame
        // ReSharper disable once UnusedMember.Local
        void Update()
        {
        }
    }
}