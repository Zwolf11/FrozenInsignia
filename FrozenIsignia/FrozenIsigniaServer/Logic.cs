using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using FrozenIsigniaClasses;

namespace FrozenIsigniaServer
{
    public class Logic
    {
        private static Random rand = new Random();

        public int id;
        public User host;
        public Map map = new Map("TestMap");
        public Dictionary<int, User> users = new Dictionary<int, User>();
        public Dictionary<int, Thread> moves = new Dictionary<int, Thread>();
        private int numStarted = 0;
        public bool lobby = true;
        private int unitID = 0;

        public Logic(int id, User host)
        {
            this.id = id;
            this.host = host;
            users.Add(host.id, host);
        }

        public void receive(User user, String[] msg)
        {
            switch (msg[0])
            {
                case "LORD":
                    placeLord(user, msg[1], msg[2]);
                    break;
                case "QUEUE":
                    Unit unit = map.units[int.Parse(msg[1])];
                    List<Location> path = new List<Location>();

                    for(int i=2;i<msg.Length;i+=2)
                        path.Add(new Location(int.Parse(msg[i]), int.Parse(msg[i+1])));

                    if(unit.player == user && unit.moves.Contains(path[path.Count - 1]))
                        queue(unit, path);

                    break;
            }
        }

        private void checkStart()
        {
            if(++numStarted >= users.Count)
            {
                spawnWave();
                map.startGame();

                foreach (User user in users.Values)
                    user.send("START");
            }
        }

        private void placeLord(User user, String lord, String squire)
        {
            Unit lordUnit = new Unit(unitID++, user, lord, new Location(map.starts[user.team - 1].x, map.starts[user.team - 1].y));
            Unit squireUnit = new Unit(unitID++, user, squire, new Location(lordUnit.loc.x + 1, lordUnit.loc.y));

            placeUnit(lordUnit);
            placeUnit(squireUnit);

            checkStart();
        }

        private void placeUnit(Unit unit)
        {
            map.placeUnit(unit);

            foreach (User player in users.Values)
                player.send("PLACE " + unit.loc.x + " " + unit.loc.y + " " + unit.ToString());
        }

        private void delaySpawnWave(int wait)
        {
            Thread.Sleep(wait);
            spawnWave();
        }

        private void spawnWave()
        {
            foreach(Location pt in map.fortresses)
            {
                if (map.tiles[pt.x][pt.y].unit == null)
                {
                    Unit unit = new Unit(unitID++);
                    unit.loc = pt;
                    placeUnit(unit);
                    Thread move = new Thread(() => delayCpuMove(unit.wait * 1000, unit));
                    moves.Add(unit.id, move);
                    move.Start();
                }
            }

            Thread wave = new Thread(() => delaySpawnWave(25 * 1000));
            wave.Start();
        }

        private void queue(Unit unit, List<Location> path)
        {
            if (moves.ContainsKey(unit.id))
            {
                moves[unit.id].Abort();
                moves.Remove(unit.id);
            }

            int wait = (int)(unit.wait * 1000 - DateTime.Now.Subtract(unit.lastAction).TotalMilliseconds);

            if (wait <= 0)
                moveUnit(unit, path);
            else
            {
                Thread move = new Thread(() => delayMoveUnit(wait, unit, path));
                moves.Add(unit.id, move);
                move.Start();
            }
        }

        private void delayCpuMove(int wait, Unit unit)
        {
            Thread.Sleep(wait);
            moves.Remove(unit.id);
            cpuMove(unit);
        }

        private void cpuMove(Unit unit)
        {
            Location loc = unit.moves[rand.Next(unit.moves.Count)];
            List<Location> path = new List<Location>();
            path.Add(new Location(unit.loc));

            if (loc != unit.loc)
            {
                int xInc = Math.Sign(loc.x - unit.loc.x);
                int yInc = Math.Sign(loc.y - unit.loc.y);

                for (int i = unit.loc.x + xInc; i != loc.x + xInc; i += xInc)
                    path.Add(new Location(i, unit.loc.y));
                for (int i = unit.loc.y + yInc; i != loc.y + yInc; i += yInc)
                    path.Add(new Location(loc.x, i));
            }

            moveUnit(unit, path);

            Thread move = new Thread(() => delayCpuMove(unit.wait * 1000, unit));
            moves.Add(unit.id, move);
            move.Start();
        }

        private void delayMoveUnit(int wait, Unit unit, List<Location> path)
        {
            Thread.Sleep(wait);
            moves.Remove(unit.id);
            moveUnit(unit, path);
        }

        private void moveUnit(Unit unit, List<Location> path)
        {
            Direction dir = unit.dir;
            if(path.Count > 1)
            {
                if (path[path.Count - 1].x > path[path.Count - 2].x)
                    dir = Direction.East;
                else if (path[path.Count - 1].x < path[path.Count - 2].x)
                    dir = Direction.West;
                else if (path[path.Count - 1].y > path[path.Count - 2].y)
                    dir = Direction.South;
                else
                    dir = Direction.North;
            }
            map.moveUnit(unit, path[path.Count - 1], dir);

            String msg = "MOVE " + unit.id;
            foreach (Location loc in path)
                msg += " " + loc.x + " " + loc.y;

            foreach (User player in users.Values)
                player.send(msg);
        }
    }
}
