using System;
using System.Collections;
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
        North = 1,
        South = 2,
        West = 3,
        East = 4
    }

    public class MazeTile
    {
        //Used for the first tile of a floor. Subsequent calls must be the second constructor
        public MazeTile(TileSpecial special, int floor, ref Maze maze)
        {
            SpecialProperty = special;
            Location = new Location(0, 0);
            maze.Floors[floor].Tiles.Add(Location, this);
        }

        public MazeTile(TileSpecial special, Direction growDirection, ref MazeTile tileBackwards, ref Maze maze)
        {
            Location newLocation;

            switch (growDirection)
            {
                case Direction.East:
                    newLocation = new Location(tileBackwards.Location.X + 1, tileBackwards.Location.Y);
                    West = tileBackwards;
                    Location = newLocation;
                    tileBackwards.East = this;
                    break;
                case Direction.North:
                    newLocation = new Location(tileBackwards.Location.X, tileBackwards.Location.Y + 1);
                    South = tileBackwards;
                    Location = newLocation;
                    tileBackwards.North = this;
                    break;
                case Direction.South:
                    newLocation = new Location(tileBackwards.Location.X, tileBackwards.Location.Y - 1);
                    North = tileBackwards;
                    Location = newLocation;
                    tileBackwards.South = this;
                    break;
                case Direction.West:
                    newLocation = new Location(tileBackwards.Location.X - 1, tileBackwards.Location.Y);
                    East = tileBackwards;
                    Location = newLocation;
                    tileBackwards.West = this;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("growDirection", growDirection, null);
            }
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
    }

    public struct Location
    {
        public int X;
        public int Y;
        
        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Floor
    {
        public Dictionary<Location, MazeTile> Tiles = new Dictionary<Location, MazeTile>();
    }

    public class Maze
    {
        public List<Floor> Floors = new List<Floor>();
    }

    public class MazeGeneration : MonoBehaviour {


        public Maze GenerateNewMaze(int floors, int tiles)
        {
            var rng = new Random();
            var maze = new Maze();

            for (int floor = 0; floor < floors; ++floor)
            {
                var previousTile = new MazeTile(TileSpecial.Nothing, floor, ref maze);
                maze.Floors[floor].Tiles.Add(previousTile.Location, previousTile);

                

                for (int tile = 0; tile < tiles; ++tile)
                {
                    var nextDirection = (Direction) rng.Next(0, 3);

                    MazeTile newTile;
                    if (nextDirection == Direction.East && maze.Floors[floor].Tiles.TryGetValue(new Location(previousTile.Location.X + 1, previousTile.Location.Y), out newTile))
                    {
                        previousTile.East = newTile;
                        newTile.West = previousTile;
                    }
                    else if (nextDirection == Direction.West && maze.Floors[floor].Tiles.TryGetValue(new Location(previousTile.Location.X - 1, previousTile.Location.Y), out newTile))
                    {
                        previousTile.West = newTile;
                        newTile.East = previousTile;
                    }
                    else if (nextDirection == Direction.North && maze.Floors[floor].Tiles.TryGetValue(new Location(previousTile.Location.X, previousTile.Location.Y + 1), out newTile))
                    {
                        previousTile.North = newTile;
                        newTile.South = previousTile;
                    }
                    else if (nextDirection == Direction.South && maze.Floors[floor].Tiles.TryGetValue(new Location(previousTile.Location.X, previousTile.Location.Y - 1), out newTile))
                    {
                        previousTile.North = newTile;
                        newTile.South = previousTile;
                    }
                    else
                    {
                        newTile = new MazeTile(TileSpecial.Nothing, nextDirection, ref previousTile, ref maze);
                        maze.Floors[floor].Tiles.Add(previousTile.Location, newTile);
                    }

                    previousTile = newTile;
                }
            }
            
            //Now we have completely generated the mazes. Let's glue them together
            for (int floor = 1; floor < floors; ++floor)
            {
                var thisFloor = maze.Floors[floor];

                var sumOfPortsToFloor = 0;
                for (int otherFloor = 0; otherFloor < floors; ++otherFloor)
                {
                    //Don't make floors back to the same one
                    if (floor == otherFloor)
                        continue;

                    var possibleDoorsOnThisFloorEast = maze.Floors[floor].Tiles.Where(e => e.Value.East == null).Select(e => e.Value.East);
                    var possibleDoorsOnThisFloorWest = maze.Floors[floor].Tiles.Where(e => e.Value.West == null).Select(e => e.Value.West);
                    var possibleDoorsOnThisFloorNorth= maze.Floors[floor].Tiles.Where(e => e.Value.North == null).Select(e => e.Value.North);
                    var possibleDoorsOnThisFloorSouth= maze.Floors[floor].Tiles.Where(e => e.Value.South == null).Select(e => e.Value.South);
                }
            }



            return maze;

        }

        // Use this for initialization
        void Start ()
        {
		
        }
	
        // Update is called once per frame
        void Update ()
        {
		
        }
    }
}