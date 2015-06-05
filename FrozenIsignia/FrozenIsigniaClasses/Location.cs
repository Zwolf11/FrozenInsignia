using System;

namespace FrozenIsigniaClasses
{
    public class Location
    {
        public int x;
        public int y;

        public Location() : this(-1, -1) { }
        public Location(Location loc) : this(loc.x, loc.y) { }

        public Location(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            Location other = (Location)obj;
            return other.x == x && other.y == y;
        }
    }
}
