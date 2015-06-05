using System;
using System.Collections.Generic;
using System.Drawing;
using FrozenIsigniaClasses;

namespace FrozenIsignia
{
    public class Move
    {
        public static float speed = 0.08f;

        public Unit unit;
        public List<Location> path;
        public PointF current;
        public int index = 0;
        public double frame = 0;
        public String anim = "walk";
        public Direction drawDir;

        public Move(Unit unit, List<Location> path)
        {
            this.unit = unit;
            this.path = path;
            current = new PointF(path[0].x, path[0].y);
            drawDir = unit.dir;
            changeDrawDir();
        }

        private void changeDrawDir()
        {
            if(index != path.Count - 1)
            {
                if (path[index + 1].x > path[index].x)
                    drawDir = Direction.East;
                else if (path[index + 1].x < path[index].x)
                    drawDir = Direction.West;
                else if (path[index + 1].y > path[index].y)
                    drawDir = Direction.South;
                else
                    drawDir = Direction.North;
            }
        }

        public bool act()
        {
            if (path.Count <= 1)
                anim = "attack";

            if (anim == "walk")
            {
                double angle = Math.Atan2(path[index+1].y - path[index].y, path[index+1].x - path[index].x);

                current.X += (float)Math.Cos(angle) * speed;
                current.Y += (float)Math.Sin(angle) * speed;
                frame = (frame + 0.25) % 8;

                if (Math.Sqrt(Math.Pow(current.X - path[index + 1].x, 2) + Math.Pow(current.Y - path[index + 1].y, 2)) < speed)
                {
                    current.X = path[index + 1].x;
                    current.Y = path[index + 1].y;

                    if (++index == path.Count - 1)
                    {
                        frame = 0;
                        anim = "attack";
                    }

                    changeDrawDir();
                }

                return false;
            }

            frame += 0.25;
            return frame >= 9;
        }
    }
}
