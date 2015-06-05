using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FrozenIsigniaClasses;

namespace FrozenIsignia
{
    public class Game : NetworkControl
    {
        private int size = 128;
        private Point cam = new Point(0, 0);
        private Point? dragLocation = null;
        private bool dragging = false;

        private Map map;
        private Unit selection = null;
        private List<Location> path = new List<Location>();
        private Unit allyCard = null;
        private Unit enemyCard = null;
        private bool showDanger = false;

        public Dictionary<int, Player> players = new Dictionary<int, Player>();
        private Dictionary<Unit, Move> moving = new Dictionary<Unit, Move>();
        private Timer timer = new Timer();

        public Game(NetworkHandler network, String mapName, Dictionary<int, Player> players) : base(network)
        {
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.Controls.Add(new LordSelector(network, 0, 0, size * 3, size));

            this.players = players;
            players.Add(-1, new Player());
            map = new Map(mapName);

            timer.Interval = 10;
            timer.Tick += this.tick;
            timer.Start();
        }

        private void tick(object sender, EventArgs e)
        {
            List<Unit> deleteList = new List<Unit>();

            foreach (Move move in moving.Values)
                if(move.act())
                    deleteList.Add(move.unit);

            foreach (Unit delete in deleteList)
                moving.Remove(delete);

            Invalidate();
        }

        public override void receive(String[] msg)
        {
            switch (msg[0])
            {
                case "START":
                    map.startGame();
                    break;
                case "PLACE":
                    Unit unit = new Unit(int.Parse(msg[3]), players[int.Parse(msg[4])], msg[5], new Location(int.Parse(msg[1]), int.Parse(msg[2])), (Direction)int.Parse(msg[6]), msg[7]);
                    unit.setStats(int.Parse(msg[8]), int.Parse(msg[9]), int.Parse(msg[10]), int.Parse(msg[11]), int.Parse(msg[12]), int.Parse(msg[13]), int.Parse(msg[14]), int.Parse(msg[15]));
                    map.placeUnit(unit);
                    break;
                case "MOVE":
                    Unit unit2 = map.units[int.Parse(msg[1])];
                    List<Location> path = new List<Location>();

                    for(int i=2;i<msg.Length;i+=2)
                        path.Add(new Location(int.Parse(msg[i]), int.Parse(msg[i+1])));

                    Direction dir = unit2.dir;
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

                    moving.Add(unit2, new Move(unit2, path));
                    map.moveUnit(unit2, path[path.Count - 1], dir);
                    break;
            }
        }

        private bool pointInPolygon(PointF pt, PointF[] pts)
        {
            PointF p1, p2;
            bool inside = false;

            if (pts.Length < 3)
                return inside;

            var oldLocation = new PointF(pts[pts.Length - 1].X, pts[pts.Length - 1].Y);

            for (int i = 0; i < pts.Length; i++)
            {
                var newLocation = new PointF(pts[i].X, pts[i].Y);

                if (newLocation.X > oldLocation.X)
                {
                    p1 = oldLocation;
                    p2 = newLocation;
                }
                else
                {
                    p1 = newLocation;
                    p2 = oldLocation;
                }

                if ((newLocation.X < pt.X) == (pt.X <= oldLocation.X) && (pt.Y - (long)p1.Y) * (p2.X - p1.X) < (p2.Y - (long)p1.Y) * (pt.X - p1.X))
                    inside = !inside;

                oldLocation = newLocation;
            }

            return inside;
        }

        private void addPathLocation(Location loc)
        {
            if (path.Count > 0 && loc != null && selection != null && selection.player.id == network.id && selection.moves.Contains(loc))
            {
                Location end = path[path.Count - 1];

                if (loc != end)
                {
                    int index = path.IndexOf(loc);

                    if (index != -1)
                        path.RemoveRange(index + 1, path.Count - index - 1);
                    else
                    {
                        int xInc = Math.Sign(loc.x - end.x);
                        int yInc = Math.Sign(loc.y - end.y);

                        for (int i = end.x + xInc; i != loc.x + xInc; i += xInc)
                            path.Add(new Location(i, end.y));
                        for (int i = end.y + yInc; i != loc.y + yInc; i += yInc)
                            path.Add(new Location(loc.x, i));
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Space:
                    showDanger = !showDanger;
                    break;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            size += Math.Sign(e.Delta);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if(dragging)
            {
                cam.X -= e.Location.X - dragLocation.Value.X;
                cam.Y -= e.Location.Y - dragLocation.Value.Y;
                dragLocation = e.Location;
            }
            else
            {
                if(dragLocation != null && Math.Sqrt(Math.Pow(e.Location.X - dragLocation.Value.X, 2) + Math.Pow(e.Location.Y - dragLocation.Value.Y, 2)) > 10)
                    dragging = true;

                Location loc = getClickTile(e.Location);

                if(loc != null)
                {
                    addPathLocation(loc);
                    Unit mouseOverUnit = map.tiles[loc.x][loc.y].unit;

                    if (mouseOverUnit != null)
                    {
                        if (mouseOverUnit.player.id == network.id)
                        {
                            if(selection == null)
                                allyCard = mouseOverUnit;
                        }
                        else
                            enemyCard = mouseOverUnit;
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!dragging)
                {
                    path.Clear();

                    Location click = getClickTile(e.Location);

                    if (click != null)
                        selection = map.tiles[click.x][click.y].unit;

                    if (selection != null)
                    {
                        allyCard = selection;
                        if(selection.player.id == network.id)
                            path.Add(click);
                    }
                    else
                    {
                        allyCard = null;
                        enemyCard = null;
                    }
                }

                dragging = false;
                dragLocation = null;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragLocation = e.Location;
            }
            else if (e.Button == MouseButtons.Right)
            {
                Location click = getClickTile(e.Location);

                if (click != null && map.started && selection.moves.Contains(click) && selection.player.id == network.id)
                {
                    if (path[path.Count - 1] != click)
                        addPathLocation(click);

                    String msg = "QUEUE " + selection.id;
                    foreach (Location loc in path)
                        msg += " " + loc.x + " " + loc.y;

                    network.send(msg);
                    path.Clear();
                    selection = null;
                }
            }
        }

        private Brush getTerrainBrush(Terrain terrain)
        {
            switch(terrain)
            {
                case Terrain.Grass:
                    return new SolidBrush(Color.FromArgb(139, 181, 74));
                case Terrain.Water:
                    return new SolidBrush(Color.FromArgb(156, 213, 226));
                case Terrain.Start:
                    return Brushes.LightGray;
                case Terrain.Fortress:
                    return new SolidBrush(Color.FromArgb(139, 181, 74));
                case Terrain.Forest:
                    return Brushes.Green;
            }

            return Brushes.White;
        }

        private Location getClickTile(Point click)
        {
            for (int i = 0; i < map.width; i++)
                for (int j = 0; j < map.height; j++)
                    if (pointInPolygon(click, getTileBounds(i, j)))
                        return new Location(i, j);

            return null;
        }

        private PointF getTileLocation(double x, double y)
        {
            float halfSize = 0.5f * size;
            float quarterSize = 0.25f * size;

            return new PointF((float)(map.height * halfSize + halfSize * x - halfSize * y - cam.X), (float)(quarterSize * (x + y + 1) - cam.Y));
        }

        private PointF[] getTileBounds(int x, int y)
        {
            float halfSize = 0.5f * size;
            float quarterSize = 0.25f * size;

            PointF pt = getTileLocation(x, y);

            return new PointF[] { new PointF(pt.X + halfSize, pt.Y), new PointF(pt.X, pt.Y - quarterSize), new PointF(pt.X - halfSize, pt.Y), new PointF(pt.X, pt.Y + quarterSize) };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            for (int i = 0; i < map.width; i++)
                for (int j = 0; j < map.height; j++)
                {
                    Location loc = new Location(i, j);
                    PointF[] tile = getTileBounds(i, j);
                    g.FillPolygon(getTerrainBrush(map.tiles[i][j].terrain), tile);

                    if(showDanger)
                        foreach(Unit enemy in map.units.Values)
                            if (enemy.player.id != network.id && enemy.attacks.Contains(loc))
                            {
                                g.FillPolygon(new SolidBrush(Color.FromArgb(100, Color.Purple)), tile);
                                break;
                            }

                    if (selection != null)
                    {
                        if (selection.moves.Contains(loc))
                            g.FillPolygon(new SolidBrush(Color.FromArgb(100, Color.CornflowerBlue)), tile);
                        else if (selection.attacks.Contains(loc))
                            g.FillPolygon(new SolidBrush(Color.FromArgb(100, Color.Red)), tile);
                    }

                    g.DrawPolygon(new Pen(Color.FromArgb(100, Color.White)), tile);
                }

            if (Images.doneLoading)
            {
                for (int i = 0; i < map.width; i++)
                    for (int j = 0; j < map.height; j++)
                    {
                        PointF pt = getTileLocation(i, j);
                        Bitmap background = Images.getTerrainBackground(map.tiles[i][j].terrain);
                        Bitmap foreground = Images.getTerrainForeground(map.tiles[i][j].terrain);
                        RectangleF bounds = new RectangleF(pt.X - 0.5f * size, pt.Y - 0.75f * size, size, size);
                        Unit unit = map.tiles[i][j].unit;

                        if (background != null)
                            g.DrawImage(background, bounds);

                        if (unit != null && !moving.ContainsKey(unit))
                            Images.drawUnit(g, unit, bounds, "idle", unit.dir, 0, true);

                        foreach (Move move in moving.Values)
                            if ((int)(move.current.X + 0.5f) == i && (int)(move.current.Y + 0.5f) == j)
                            {
                                PointF pt2 = getTileLocation(move.current.X, move.current.Y);
                                RectangleF bounds2 = new RectangleF(pt2.X - 0.5f * size, pt2.Y - 0.75f * size, size, size);
                                Images.drawUnit(g, move.unit, bounds2, move.anim, move.drawDir, (int)move.frame, true);
                            }

                        if (foreground != null)
                            g.DrawImage(foreground, bounds);
                    }

                if (allyCard != null)
                    Images.drawCard(g, allyCard, new RectangleF(10, ClientSize.Height - 202, 256, 192));

                if(enemyCard != null)
                    Images.drawCard(g, enemyCard, new RectangleF(ClientSize.Width - 266, ClientSize.Height - 202, 256, 192));
            }

            for(int i=0;i<path.Count;i++)
            {
                PointF pt = getTileLocation(path[i].x, path[i].y);

                g.FillEllipse(Brushes.AliceBlue, pt.X - size / 32f, pt.Y - size / 32f, size / 16f, size / 16f);

                if (i != path.Count - 1)
                    g.DrawLine(Pens.AliceBlue, pt, getTileLocation(path[i + 1].x, path[i + 1].y));
            }
        }
    }
}
