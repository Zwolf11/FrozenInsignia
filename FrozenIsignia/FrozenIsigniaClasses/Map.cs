using System;
using System.Collections.Generic;
using System.Drawing;

namespace FrozenIsigniaClasses
{
    public enum Direction { East, South, West, North }

    public class Map
    {
        public String name;
        public int width;
        public int height;
        public Tile[][] tiles;
        public Dictionary<int, Unit> units = new Dictionary<int, Unit>();
        public bool started = false;

        public List<Location> starts;
        public List<Location> fortresses;
        public int maxPlayers { get { return starts.Count; } }

        public Map(String map)
        {
            switch (map)
            {
                case "TestMap":
                    buildTestMap();
                    break;
            }

            fillLists();
        }

        public void startGame()
        {
            DateTime now = DateTime.Now;

            foreach (Unit unit in units.Values)
                unit.lastAction = now;

            started = true;
        }

        public void placeUnit(Unit unit)
        {
            tiles[unit.loc.x][unit.loc.y].unit = unit;
            units.Add(unit.id, unit);
            setUnitLists(unit);

            foreach (Unit unit2 in units.Values)
                if (unit2.moves.Contains(unit.loc))
                    setUnitLists(unit2);
        }

        public void moveUnit(Unit unit, Location loc, Direction dir)
        {
            unit.lastAction = DateTime.Now;
            unit.dir = dir;

            if (loc.x != unit.loc.x || loc.y != unit.loc.y)
            {
                Location origLoc = new Location(unit.loc);
                tiles[loc.x][loc.y].unit = unit;
                tiles[unit.loc.x][unit.loc.y].unit = null;
                unit.loc = loc;

                foreach (Unit unit2 in units.Values)
                    //if (unit2.moves.Contains(loc) || unit2.moves.Contains(origLoc))
                        setUnitLists(unit2);
            }
        }

        private void fillLists()
        {
            starts = new List<Location>();
            fortresses = new List<Location>();

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (tiles[i][j].terrain == Terrain.Start)
                        starts.Add(new Location(i, j));
                    else if (tiles[i][j].terrain == Terrain.Fortress)
                        fortresses.Add(new Location(i, j));
                }
        }

        private bool validLocation(Location loc)
        {
            return loc.x >= 0 && loc.x < width && loc.y >= 0 && loc.y < height;
        }

        private void addVision(Unit unit, int index)
        {
            if (index >= unit.vision.Count)
                return;

            Location loc = unit.vision[index];
            int movesLeft = tiles[loc.x][loc.y].movesLeft;

            Location right = new Location(loc.x + 1, loc.y);
            Location up = new Location(loc.x, loc.y + 1);
            Location left = new Location(loc.x - 1, loc.y);
            Location down = new Location(loc.x, loc.y - 1);

            if (validLocation(right))
            {
                Tile tile = tiles[right.x][right.y];

                if (tile.movesLeft < 0)
                {
                    tile.movesLeft = movesLeft - 1;

                    if (tile.movesLeft >= 0)
                        unit.vision.Add(right);
                }
            }

            if (validLocation(up))
            {
                Tile tile = tiles[up.x][up.y];

                if (tile.movesLeft < 0)
                {
                    tile.movesLeft = movesLeft - 1;

                    if (tile.movesLeft >= 0)
                        unit.vision.Add(up);
                }
            }

            if (validLocation(left))
            {
                Tile tile = tiles[left.x][left.y];

                if (tile.movesLeft < 0)
                {
                    tile.movesLeft = movesLeft - 1;

                    if (tile.movesLeft >= 0)
                        unit.vision.Add(left);
                }
            }

            if (validLocation(down))
            {
                Tile tile = tiles[down.x][down.y];

                if (tile.movesLeft < 0)
                {
                    tile.movesLeft = movesLeft - 1;

                    if (tile.movesLeft >= 0)
                        unit.vision.Add(down);
                }
            }

            addVision(unit, index + 1);
        }

        private void addMoves(Unit unit, int index)
        {
            if (index >= unit.moves.Count)
                return;

            Location loc = unit.moves[index];
            int movesLeft = tiles[loc.x][loc.y].movesLeft;

            Location right = new Location(loc.x + 1, loc.y);
            Location up = new Location(loc.x, loc.y + 1);
            Location left = new Location(loc.x - 1, loc.y);
            Location down = new Location(loc.x, loc.y - 1);

            if(validLocation(right))
            {
                Tile tile = tiles[right.x][right.y];

                if (tile.movesLeft < 0)
                {
                    tile.movesLeft = movesLeft - tile.multiplier;

                    if (tile.movesLeft >= 0 && tile.multiplier != 0 && (tile.unit == null || tile.unit.player == unit.player))
                        unit.moves.Add(right);
                }
            }

            if (validLocation(up))
            {
                Tile tile = tiles[up.x][up.y];

                if (tile.movesLeft < 0)
                {
                    tile.movesLeft = movesLeft - tile.multiplier;

                    if (tile.movesLeft >= 0 && tile.multiplier != 0 && (tile.unit == null || tile.unit.player == unit.player))
                        unit.moves.Add(up);
                }
            }

            if (validLocation(left))
            {
                Tile tile = tiles[left.x][left.y];

                if (tile.movesLeft < 0)
                {
                    tile.movesLeft = movesLeft - tile.multiplier;

                    if (tile.movesLeft >= 0 && tile.multiplier != 0 && (tile.unit == null || tile.unit.player == unit.player))
                        unit.moves.Add(left);
                }
            }

            if (validLocation(down))
            {
                Tile tile = tiles[down.x][down.y];

                if (tile.movesLeft < 0)
                {
                    tile.movesLeft = movesLeft - tile.multiplier;

                    if (tile.movesLeft >= 0 && tile.multiplier != 0 && (tile.unit == null || tile.unit.player == unit.player))
                        unit.moves.Add(down);
                }
            }

            addMoves(unit, index + 1);
        }

        private void addAttacks(Unit unit)
        {
            foreach(Location loc in unit.moves)
            {
                for (int i = unit.minRange; i <= unit.maxRange; i++)
                {
                    Location right = new Location(loc.x + i, loc.y);
                    Location up = new Location(loc.x, loc.y + i);
                    Location left = new Location(loc.x - i, loc.y);
                    Location down = new Location(loc.x, loc.y - i);

                    if (validLocation(right) && !unit.attacks.Contains(right))
                        unit.attacks.Add(right);
                    if (validLocation(up) && !unit.attacks.Contains(up))
                        unit.attacks.Add(up);
                    if (validLocation(left) && !unit.attacks.Contains(left))
                        unit.attacks.Add(left);
                    if (validLocation(down) && !unit.attacks.Contains(down))
                        unit.attacks.Add(down);
                }
            }
        }

        public void setUnitLists(Unit unit)
        {
            unit.vision.Clear();
            unit.vision.Add(unit.loc);
            tiles[unit.loc.x][unit.loc.y].movesLeft = unit.sight;
            addVision(unit, 0);
            resetMoves();

            unit.moves.Clear();
            unit.moves.Add(unit.loc);
            tiles[unit.loc.x][unit.loc.y].movesLeft = unit.mov;
            addMoves(unit, 0);
            resetMoves();

            unit.attacks.Clear();
            addAttacks(unit);
        }

        private void buildTestMap()
        {
            name = "TestMap";
            width = 20;
            height = 20;
            tiles = new Tile[width][];

            for (int i = 0; i < width; i++)
            {
                tiles[i] = new Tile[height];
                for (int j = 0; j < height; j++)
                    tiles[i][j] = new Tile(Terrain.Grass);
            }

            tiles[2][0] = new Tile(Terrain.Water);
            tiles[10][10] = new Tile(Terrain.Fortress);
            tiles[0][0] = new Tile(Terrain.Start);
            tiles[10][0] = new Tile(Terrain.Start);
            tiles[0][10] = new Tile(Terrain.Start);
            tiles[5][5] = new Tile(Terrain.Start);
            tiles[11][15] = new Tile(Terrain.Forest);
            tiles[4][3] = new Tile(Terrain.Forest);
        }

        public void resetMoves()
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    tiles[i][j].movesLeft = -1;
        }
    }
}
