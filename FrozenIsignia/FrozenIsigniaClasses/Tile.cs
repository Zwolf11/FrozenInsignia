using System;
using System.Collections.Generic;
using System.Linq;

namespace FrozenIsigniaClasses
{
    public enum Terrain { Grass, Water, Fortress, Start, Forest }

    public class Tile
    {
        public Terrain terrain;
        public int multiplier;
        public Unit unit;
        public List<Unit> vision = new List<Unit>();
        public int movesLeft = -1;

        public Tile(Terrain type)
        {
            this.terrain = type;

            switch(type)
            {
                case Terrain.Grass:
                    multiplier = 1;
                    break;
                case Terrain.Water:
                    multiplier = 0;
                    break;
                case Terrain.Fortress:
                    multiplier = 2;
                    break;
                case Terrain.Start:
                    multiplier = 1;
                    break;
                case Terrain.Forest:
                    multiplier = 3;
                    break;
            }
        }
    }
}
