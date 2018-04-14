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
        PlayerSpawn = 1,


        Key1 = 1001,
        Key2 = 1002,
        Key3 = 1003,


        BreakingPoint1 = 2001
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
    }

    public struct Location
    {
        public int X;
        public int Z;

        public Location(int x, int z)
        {
            X = x;
            Z = z;
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
    }

    public static class HelperFunctions
    {
        
    }

    public class MazeGeneration : MonoBehaviour
    {
        public int Floors = 1;
        public int TilesPerFloor = 30;

        public Maze GenerateNewMaze(int? floors = null, int? tilesPerFloor = null)
        {
            //Get optional params
            Floors = floors ?? Floors;
            TilesPerFloor = tilesPerFloor ?? TilesPerFloor;

            //Set up the maze
            var rng = new Random();
            var maze = new Maze();

            maze.Floors.AddRange(Enumerable.Repeat<Floor>(null, Floors).ToList());

            for (int floor = 0; floor < Floors; ++floor)
            {
                var previousTile = new MazeTile(TileSpecial.Nothing, floor, ref maze);


                for (int tile = 0; tile < TilesPerFloor; ++tile)
                {
                    var nextDirection = (Direction) rng.Next(0, 3);

                    MazeTile newTile;
                    if (nextDirection == Direction.East && maze.Floors[floor].Tiles
                            .TryGetValue(new Location(previousTile.Location.X + 1, previousTile.Location.Z),
                                out newTile))
                    {
                        previousTile.East = newTile;
                        newTile.West = previousTile;
                    }
                    else if (nextDirection == Direction.West && maze.Floors[floor].Tiles
                                 .TryGetValue(new Location(previousTile.Location.X - 1, previousTile.Location.Z),
                                     out newTile))
                    {
                        previousTile.West = newTile;
                        newTile.East = previousTile;
                    }
                    else if (nextDirection == Direction.North && maze.Floors[floor].Tiles
                                 .TryGetValue(new Location(previousTile.Location.X, previousTile.Location.Z + 1),
                                     out newTile))
                    {
                        previousTile.North = newTile;
                        newTile.South = previousTile;
                    }
                    else if (nextDirection == Direction.South && maze.Floors[floor].Tiles
                                 .TryGetValue(new Location(previousTile.Location.X, previousTile.Location.Z - 1),
                                     out newTile))
                    {
                        previousTile.North = newTile;
                        newTile.South = previousTile;
                    }
                    else
                    {
                        newTile = new MazeTile(TileSpecial.Nothing, nextDirection, ref previousTile, ref maze);
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
                        maze.Floors[floor].Tiles.Where(e => e.Value.East == null).ToList();
                    var possibleDoorsOnOtherFloorWest =
                        maze.Floors[floor].Tiles.Where(e => e.Value.West == null).ToList();
                    var possibleDoorsOnOtherFloorNorth =
                        maze.Floors[floor].Tiles.Where(e => e.Value.North == null).ToList();
                    var possibleDoorsOnOtherFloorSouth =
                        maze.Floors[floor].Tiles.Where(e => e.Value.South == null).ToList();

                    int toOtherFloors = maze.Floors[floor].NumPortsToOtherFloors == 0 ? rng.Next(1, 3) : rng.Next(0, 3);

                    for (int portal = 0; portal < toOtherFloors; ++portal)
                    {
                        maze.Floors[floor].NumPortsToOtherFloors++;
                        maze.Floors[otherFloor].NumPortsToOtherFloors++;
                        var randomDirectionFromThisFloor = (Direction) rng.Next(0, 3);
                        if (randomDirectionFromThisFloor == Direction.East
                            && possibleDoorsOnThisFloorEast.Count > 0
                            && possibleDoorsOnOtherFloorWest.Count > 0)
                        {
                            var idxFromThisFloor = rng.Next(0, possibleDoorsOnThisFloorEast.Count);
                            var idxFromOtherFloor = rng.Next(0, possibleDoorsOnOtherFloorWest.Count);
                            possibleDoorsOnThisFloorEast[idxFromThisFloor].Value.East =
                                possibleDoorsOnOtherFloorWest[idxFromOtherFloor].Value;
                            possibleDoorsOnThisFloorWest[idxFromOtherFloor].Value.West =
                                possibleDoorsOnOtherFloorEast[idxFromThisFloor].Value;
                            possibleDoorsOnThisFloorEast.RemoveAt(idxFromThisFloor);
                            possibleDoorsOnThisFloorWest.RemoveAt(idxFromOtherFloor);
                        }
                        else if (randomDirectionFromThisFloor == Direction.West
                                 && possibleDoorsOnThisFloorWest.Count > 0
                                 && possibleDoorsOnOtherFloorEast.Count > 0)
                        {
                            var idxFromThisFloor = rng.Next(0, possibleDoorsOnThisFloorWest.Count);
                            var idxFromOtherFloor = rng.Next(0, possibleDoorsOnOtherFloorEast.Count);
                            possibleDoorsOnThisFloorWest[idxFromThisFloor].Value.West =
                                possibleDoorsOnOtherFloorEast[idxFromOtherFloor].Value;
                            possibleDoorsOnThisFloorEast[idxFromOtherFloor].Value.East =
                                possibleDoorsOnOtherFloorWest[idxFromThisFloor].Value;
                            possibleDoorsOnThisFloorWest.RemoveAt(idxFromThisFloor);
                            possibleDoorsOnThisFloorEast.RemoveAt(idxFromOtherFloor);
                        }
                        else if (randomDirectionFromThisFloor == Direction.North
                                 && possibleDoorsOnThisFloorNorth.Count > 0
                                 && possibleDoorsOnOtherFloorSouth.Count > 0)
                        {
                            var idxFromThisFloor = rng.Next(0, possibleDoorsOnThisFloorNorth.Count);
                            var idxFromOtherFloor = rng.Next(0, possibleDoorsOnOtherFloorSouth.Count);
                            possibleDoorsOnThisFloorNorth[idxFromThisFloor].Value.North =
                                possibleDoorsOnOtherFloorSouth[idxFromOtherFloor].Value;
                            possibleDoorsOnThisFloorSouth[idxFromOtherFloor].Value.South =
                                possibleDoorsOnOtherFloorNorth[idxFromThisFloor].Value;
                            possibleDoorsOnThisFloorNorth.RemoveAt(idxFromThisFloor);
                            possibleDoorsOnThisFloorSouth.RemoveAt(idxFromOtherFloor);
                        }
                        else if (randomDirectionFromThisFloor == Direction.South
                                 && possibleDoorsOnThisFloorSouth.Count > 0
                                 && possibleDoorsOnOtherFloorNorth.Count > 0)
                        {
                            var idxFromThisFloor = rng.Next(0, possibleDoorsOnThisFloorSouth.Count);
                            var idxFromOtherFloor = rng.Next(0, possibleDoorsOnOtherFloorNorth.Count);
                            possibleDoorsOnThisFloorSouth[idxFromThisFloor].Value.South =
                                possibleDoorsOnOtherFloorNorth[idxFromOtherFloor].Value;
                            possibleDoorsOnThisFloorNorth[idxFromOtherFloor].Value.North =
                                possibleDoorsOnOtherFloorSouth[idxFromThisFloor].Value;
                            possibleDoorsOnThisFloorSouth.RemoveAt(idxFromThisFloor);
                            possibleDoorsOnThisFloorNorth.RemoveAt(idxFromOtherFloor);
                        }

                    }
                }
            }

            //Add a start position to a southern wall somewhere
            for (var i = 0; i < 3 /*Number of directions*/; ++i)
            {
                var usingDirection = (Direction)i;
                var listOfValidStartTiles = maze.Floors.SelectMany(e => e.Tiles.Select(a => a.Value))
                    .Where(e => e.SideByDirection(usingDirection) == null 
                            && !maze.Floors[e.Floor].Tiles.ContainsKey(e.Location.AddDirectionOnce(usingDirection))).ToList();
                if (listOfValidStartTiles.Count == 0) continue;
                var aValidTile = listOfValidStartTiles[rng.Next(listOfValidStartTiles.Count)];

                maze.StartTile = new MazeTile(TileSpecial.PlayerSpawn, usingDirection, ref aValidTile, ref maze);
            }
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
                    var maze = GenerateNewMaze();
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