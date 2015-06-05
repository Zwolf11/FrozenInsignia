using System;
using System.Drawing;

namespace FrozenIsigniaClasses
{
    public class Player
    {
        public int id;
        public String name;
        public int team;

        public Player() : this(-1) { }
        public Player(int id) : this(id, "Mingebag") { }
        public Player(int id, String name) : this(id, name, 0) { }

        public Player(int id, String name, int team)
        {
            this.id = id;
            this.name = name;
            this.team = team;
        }

        public override bool Equals(object obj)
        {
            Player other = (Player)obj;
            return other.id == id;
        }
    }
}
