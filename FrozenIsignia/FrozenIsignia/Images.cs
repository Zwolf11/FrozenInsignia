using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using FrozenIsigniaClasses;

namespace FrozenIsignia
{
    public class Attachment
    {
        public PointF pt;
        public float angle;

        public Attachment(PointF pt, float angle)
        {
            this.pt = pt;
            this.angle = angle;
        }
    }

    public static class Images
    {
        public static bool doneLoading { get { return idleLoaded && walkLoaded && attackLoaded && miscLoaded; } }
        private static bool idleLoaded = false;
        private static bool walkLoaded = false;
        private static bool attackLoaded = false;
        private static bool miscLoaded = false;

        public static Color[] teams;

        public static Bitmap[][] idleBody;
        public static Bitmap[][] idleHand;
        public static Attachment idleWeaponAttach;

        public static Bitmap[][][] walkBody;
        public static Bitmap[][][] walkHand;
        public static Attachment[] walkWeaponAttach;

        public static Bitmap[][][] attackBody;
        public static Bitmap[][][] attackHand;
        public static Attachment[] attackWeaponAttach;

        public static Bitmap[] weapons;

        public static Bitmap[] terrainForeground;
        public static Bitmap[] terrainBackground;

        public static Font largeFont = new Font("Arial", 30, GraphicsUnit.Pixel);
        public static Font mediumFont = new Font("Arial", 18, GraphicsUnit.Pixel);
        public static Font smallFont = new Font("Arial", 12, GraphicsUnit.Pixel);

        public static Bitmap getTerrainForeground(Terrain terrain)
        {
            switch(terrain)
            {
                case Terrain.Forest:
                    return terrainForeground[0];
                case Terrain.Fortress:
                    return terrainForeground[1];
            }

            return null;
        }

        public static Bitmap getTerrainBackground(Terrain terrain)
        {
            switch (terrain)
            {
                case Terrain.Forest:
                    return terrainBackground[0];
                case Terrain.Fortress:
                    return terrainBackground[1];
            }

            return null;
        }

        private static void drawWeapon(Graphics g, Unit unit, RectangleF bounds, String anim, Direction drawDir, int frame)
        {
            Bitmap weapon = weapons[0];
            PointF attach = new PointF();
            float angle = 0;

            switch(unit.weapon)
            {
                case Weapon.Sword:
                    weapon = weapons[0];
                    break;
                case Weapon.Axe:
                    weapon = weapons[1];
                    break;
                case Weapon.Lance:
                    weapon = weapons[2];
                    break;
                case Weapon.Staff:
                    weapon = weapons[3];
                    break;
            }

            switch(anim)
            {
                case "idle":
                    attach = idleWeaponAttach.pt;
                    angle = idleWeaponAttach.angle;
                    break;
                case "walk":
                    attach = walkWeaponAttach[frame].pt;
                    angle = walkWeaponAttach[frame].angle;
                    break;
                case "attack":
                    attach = attackWeaponAttach[frame].pt;
                    angle = attackWeaponAttach[frame].angle;
                    break;
            }

            if (drawDir == Direction.West || drawDir == Direction.South)
            {
                weapon = flipBitmap(weapon);
                angle = -angle;
                attach.X = -attach.X;
            }
            
            g.DrawImage(rotateBitmap(weapon, angle), bounds.X + attach.X * bounds.Width, bounds.Y + attach.Y * bounds.Height, bounds.Width, bounds.Height);
        }

        public static void drawUnit(Graphics g, Unit unit, RectangleF bounds, String anim, Direction drawDir, int frame, bool drawBars)
        {
            switch(anim)
            {
                case "idle":
                    g.DrawImage(idleBody[unit.player.team][(int)drawDir], bounds);
                    drawWeapon(g, unit, bounds, anim, drawDir, frame);
                    g.DrawImage(idleHand[unit.player.team][(int)drawDir], bounds);
                    break;
                case "walk":
                    if(frame == 3 || frame == 7)
                    {
                        g.DrawImage(idleBody[unit.player.team][(int)drawDir], bounds);
                        drawWeapon(g, unit, bounds, "idle", drawDir, 0);
                        g.DrawImage(idleHand[unit.player.team][(int)drawDir], bounds);
                    }
                    else if(frame == 0 || frame == 2)
                    {
                        g.DrawImage(walkBody[0][unit.player.team][(int)drawDir], bounds);
                        drawWeapon(g, unit, bounds, anim, drawDir, 0);
                        g.DrawImage(walkHand[0][unit.player.team][(int)drawDir], bounds);
                    }
                    else if (frame == 1)
                    {
                        g.DrawImage(walkBody[1][unit.player.team][(int)drawDir], bounds);
                        drawWeapon(g, unit, bounds, anim, drawDir, 1);
                        g.DrawImage(walkHand[1][unit.player.team][(int)drawDir], bounds);
                    }
                    else if (frame == 4 || frame == 6)
                    {
                        g.DrawImage(walkBody[2][unit.player.team][(int)drawDir], bounds);
                        drawWeapon(g, unit, bounds, anim, drawDir, 2);
                        g.DrawImage(walkHand[2][unit.player.team][(int)drawDir], bounds);
                    }
                    else if (frame == 5)
                    {
                        g.DrawImage(walkBody[3][unit.player.team][(int)drawDir], bounds);
                        drawWeapon(g, unit, bounds, anim, drawDir, 3);
                        g.DrawImage(walkHand[3][unit.player.team][(int)drawDir], bounds);
                    }
                    break;
                case "attack":
                    g.DrawImage(attackBody[frame][unit.player.team][(int)drawDir], bounds);
                    drawWeapon(g, unit, bounds, anim, drawDir, frame);
                    g.DrawImage(attackHand[frame][unit.player.team][(int)drawDir], bounds);
                    break;
            }

            if (drawBars)
            {
                float inset = bounds.Width / 4f;
                float width = bounds.Width - inset * 2;
                float height = bounds.Width / 16f;
                float hpPercent = 1f * unit.hpCurrent / unit.hp;
                float waitPercent = (float)(DateTime.Now.Subtract(unit.lastAction).TotalSeconds / unit.wait);
                waitPercent = waitPercent > 1 ? 1 : waitPercent;

                g.FillRectangle(Brushes.Red, bounds.X + inset, bounds.Y - 3 * height, hpPercent * width, height);
                g.FillRectangle(Brushes.White, bounds.X + inset, bounds.Y - 2 * height, waitPercent * width, height);
            }
        }

        public static void drawCard(Graphics g, Unit unit, RectangleF bounds)
        {
            Color color;
            Color light;
            Color lighter;
            Color dark;

            if(unit.player.team == 0)
            {
                color = Color.FromArgb(0, 0, 0);
                light = Color.FromArgb(127, 127, 127);
                lighter = Color.FromArgb(164, 164, 164);
                dark = Color.FromArgb(100, 100, 100);
            }
            else
            {
                color = Images.teams[unit.player.team - 1];
                light = Color.FromArgb(color.R + 127 > 255 ? 255 : color.R + 127, color.G + 127 > 255 ? 255 : color.G + 127, color.B + 127 > 255 ? 255 : color.B + 127);
                lighter = Color.FromArgb(light.R + 37 > 255 ? 255 : light.R + 37, light.G + 37 > 255 ? 255 : light.G + 37, light.B + 37 > 255 ? 255 : light.B + 37);
                dark = Color.FromArgb(light.R - 27 < 0 ? 0 : light.R - 27, light.G - 27 < 0 ? 0 : light.G - 27, light.B - 27 < 0 ? 0 : light.B - 27);
            }

            int radius = 10;
            GraphicsPath p = new GraphicsPath();
            p.AddArc(bounds.X, bounds.Y, 2 * radius, 2 * radius, 180, 90);
            p.AddLine(bounds.X + radius, bounds.Y, bounds.Right - radius, bounds.Y);
            p.AddArc(bounds.Right - 2 * radius, bounds.Y, 2 * radius, 2 * radius, 270, 90);
            p.AddLine(bounds.Right, bounds.Y + radius, bounds.Right, bounds.Bottom - radius);
            p.AddArc(bounds.Right - 2 * radius, bounds.Bottom - 2 * radius, 2 * radius, 2 * radius, 0, 90);
            p.AddLine(bounds.Right - radius, bounds.Bottom, bounds.X + radius, bounds.Bottom);
            p.AddArc(bounds.X, bounds.Bottom - 2 * radius, 2 * radius, 2 * radius, 90, 90);
            p.AddLine(bounds.X, bounds.Bottom - radius, bounds.X, bounds.Bottom);
            p.CloseFigure();

            Pen pen = new Pen(dark);
            SolidBrush brush = new SolidBrush(Color.FromArgb(53, 53, 53));
            StringFormat rightAlign = new StringFormat();
            rightAlign.Alignment = StringAlignment.Far;
            StringFormat centerAlign = new StringFormat();
            centerAlign.Alignment = StringAlignment.Center;
            pen.Alignment = PenAlignment.Inset;
            pen.Width = 3;
            g.FillPath(new SolidBrush(lighter), p);
            g.DrawPath(pen, p);
            String range = unit.minRange == unit.maxRange ? "" + unit.minRange : unit.minRange + "-" + unit.maxRange;

            float unitSize = bounds.Width / 2f;
            drawUnit(g, unit, new RectangleF(bounds.X - 0.1796875f * unitSize, bounds.Y + 0.0625f * unitSize, unitSize, unitSize), "idle", Direction.East, 0, false);
            g.DrawString(unit.name, largeFont, brush, bounds.X + 0.3203125f * bounds.Width, bounds.Y + 0.0625f * bounds.Height);
            g.DrawString(unit.type, mediumFont, brush, bounds.X + 0.33984375f * bounds.Width, bounds.Y + 0.203125f * bounds.Height);
            g.DrawString("Move:\nSight:\nWait:\nRange:", smallFont, brush, bounds.X + 0.33984375f * bounds.Width, bounds.Y + 0.3177083f * bounds.Height);
            g.DrawString("Damage:\nSpeed:\nDefense:\nResistance:", smallFont, brush, bounds.X + 0.64453125f * bounds.Width, bounds.Y + 0.3177083f * bounds.Height);
            g.DrawString(unit.mov + "\n" + unit.sight + "\n" + unit.wait + "\n" + range, smallFont, brush, bounds.X + 0.58203125f * bounds.Width, bounds.Y + 0.3177083f * bounds.Height, rightAlign);
            g.DrawString(unit.dmg + "\n" + unit.spd + "\n" + unit.def + "\n" + unit.res, smallFont, brush, bounds.X + 0.9609375f * bounds.Width, bounds.Y + 0.3177083f * bounds.Height, rightAlign);
            g.DrawString("Special: " + unit.special + " - " + unit.specDesc, smallFont, brush, new RectangleF(bounds.X + 0.03515625f * bounds.Width, bounds.Y + 0.640625f * bounds.Height, 0.93359375f * bounds.Width, 30), centerAlign);
        }

        private static void loadIdle(Color[] teams)
        {
            int numColors = teams.Length + 1;

            idleBody = new Bitmap[numColors][];
            idleHand = new Bitmap[numColors][];
            
            for (int i = 0; i < numColors; i++)
            {
                idleBody[i] = new Bitmap[4];
                idleHand[i] = new Bitmap[4];
            }

            idleBody[0][0] = new Bitmap("Assets/character/idle/body.png");
            idleHand[0][0] = new Bitmap("Assets/character/idle/hand.png");

            for (int i = 1; i < numColors; i++)
            {
                idleBody[i][0] = colorizeBitmap(idleBody[0][0], teams[i - 1]);
                idleHand[i][0] = colorizeBitmap(idleHand[0][0], teams[i - 1]);
            }

            for (int i = 0; i < numColors; i++)
            {
                idleBody[i][1] = flipBitmap(idleBody[i][0]);
                idleHand[i][1] = flipBitmap(idleHand[i][0]);

                idleBody[i][2] = idleBody[i][1];
                idleHand[i][2] = idleHand[i][1];

                idleBody[i][3] = idleBody[i][0];
                idleHand[i][3] = idleHand[i][0];
            }

            idleWeaponAttach = new Attachment(new PointF(-0.15625f, 0.1015625f), 11.8f);

            idleLoaded = true;
        }

        private static void loadWalk(Color[] teams)
        {
            int numColors = teams.Length + 1;

            walkBody = new Bitmap[4][][];
            walkHand = new Bitmap[4][][];

            for (int i = 0; i < walkBody.Length; i++)
            {
                walkBody[i] = new Bitmap[numColors][];
                walkHand[i] = new Bitmap[numColors][];

                for (int j = 0; j < numColors; j++)
                {
                    walkBody[i][j] = new Bitmap[4];
                    walkHand[i][j] = new Bitmap[4];
                }

                walkBody[i][0][0] = new Bitmap("Assets/character/walk_" + i + "/body.png");
                walkHand[i][0][0] = new Bitmap("Assets/character/walk_" + i + "/hand.png");

                for (int j = 1; j < numColors; j++)
                {
                    walkBody[i][j][0] = colorizeBitmap(walkBody[i][0][0], teams[j - 1]);
                    walkHand[i][j][0] = colorizeBitmap(walkHand[i][0][0], teams[j - 1]);
                }

                for(int j=0;j<numColors;j++)
                {
                    walkBody[i][j][1] = flipBitmap(walkBody[i][j][0]);
                    walkHand[i][j][1] = flipBitmap(walkHand[i][j][0]);

                    walkBody[i][j][2] = walkBody[i][j][1];
                    walkHand[i][j][2] = walkHand[i][j][1];

                    walkBody[i][j][3] = walkBody[i][j][0];
                    walkHand[i][j][3] = walkHand[i][j][0];
                }
            }

            walkWeaponAttach = new Attachment[4];
            walkWeaponAttach[0] = new Attachment(new PointF(-0.2265625f, 0.0625f), 25.5f);
            walkWeaponAttach[1] = new Attachment(new PointF(-0.234375f, 0.0625f), 36.4f);
            walkWeaponAttach[2] = new Attachment(new PointF(-0.1015625f, 0.109375f), -18.4f);
            walkWeaponAttach[3] = new Attachment(new PointF(-0.03125f, 0.109375f), -33.2f);

            walkLoaded = true;
        }

        private static void loadAttack(Color[] teams)
        {
            int numColors = teams.Length + 1;

            attackBody = new Bitmap[9][][];
            attackHand = new Bitmap[9][][];

            for (int i = 0; i < attackBody.Length; i++)
            {
                attackBody[i] = new Bitmap[numColors][];
                attackHand[i] = new Bitmap[numColors][];

                for (int j = 0; j < numColors; j++)
                {
                    attackBody[i][j] = new Bitmap[4];
                    attackHand[i][j] = new Bitmap[4];
                }

                attackBody[i][0][0] = new Bitmap("Assets/character/attack_" + i + "/body.png");
                attackHand[i][0][0] = new Bitmap("Assets/character/attack_" + i + "/hand.png");

                for (int j = 1; j < numColors; j++)
                {
                    attackBody[i][j][0] = colorizeBitmap(attackBody[i][0][0], teams[j - 1]);
                    attackHand[i][j][0] = colorizeBitmap(attackHand[i][0][0], teams[j - 1]);
                }

                for (int j = 0; j < numColors; j++)
                {
                    attackBody[i][j][1] = flipBitmap(attackBody[i][j][0]);
                    attackHand[i][j][1] = flipBitmap(attackHand[i][j][0]);

                    attackBody[i][j][2] = attackBody[i][j][1];
                    attackHand[i][j][2] = attackHand[i][j][1];

                    attackBody[i][j][3] = attackBody[i][j][0];
                    attackHand[i][j][3] = attackHand[i][j][0];
                }
            }

            attackWeaponAttach = new Attachment[9];
            attackWeaponAttach[0] = new Attachment(new PointF(-0.09375f, 0.09375f), -19.1f);
            attackWeaponAttach[1] = new Attachment(new PointF(-0.03125f, 0.109375f), -34.8f);
            attackWeaponAttach[2] = new Attachment(new PointF(0.046875f, 0.078125f), -68.2f);
            attackWeaponAttach[3] = new Attachment(new PointF(0.1171875f, 0.015625f), -112.2f);
            attackWeaponAttach[4] = new Attachment(new PointF(0.09375f, -0.0546875f), -142.7f);
            attackWeaponAttach[5] = new Attachment(new PointF(0f, -0.0703125f), -180f);
            attackWeaponAttach[6] = new Attachment(new PointF(0.109375f, 0.0078125f), -123f);
            attackWeaponAttach[7] = new Attachment(new PointF(0.1015625f, 0.09375f), -69.1f);
            attackWeaponAttach[8] = new Attachment(new PointF(-0.015625f, 0.171875f), 0f);

            attackLoaded = true;
        }

        public static void loadImgs(Color[] teams)
        {
            new Thread(() => loadIdle(teams)).Start();
            new Thread(() => loadWalk(teams)).Start();
            new Thread(() => loadAttack(teams)).Start();

            Images.teams = teams;

            weapons = new Bitmap[4];
            weapons[0] = new Bitmap("Assets/weapons/sword.png");
            weapons[1] = new Bitmap("Assets/weapons/axe.png");
            weapons[2] = new Bitmap("Assets/weapons/lance.png");
            weapons[3] = new Bitmap("Assets/weapons/staff.png");

            terrainForeground = new Bitmap[2];
            terrainBackground = new Bitmap[2];

            terrainForeground[0] = new Bitmap("Assets/terrain/forest/foreground.png");
            terrainForeground[1] = new Bitmap("Assets/terrain/fortress/foreground.png");

            terrainBackground[0] = new Bitmap("Assets/terrain/forest/background.png");
            terrainBackground[1] = new Bitmap("Assets/terrain/fortress/background.png");

            miscLoaded = true;
        }

        public static Bitmap flipBitmap(Bitmap img)
        {
            Bitmap bitmap = new Bitmap(img);
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipY);
            return bitmap;
        }

        public static Bitmap rotateBitmap(Bitmap img, float angle)
        {
            Bitmap bitmap = new Bitmap(img.Width, img.Height);
            Graphics g = Graphics.FromImage(bitmap);

            float midWidth = img.Width / 2f;
            float midHeight = img.Height / 2f;

            g.TranslateTransform(midWidth, midHeight);
            g.RotateTransform(angle);
            g.TranslateTransform(-midWidth, -midHeight);
            g.DrawImage(img, 0, 0, img.Width, img.Height);

            return bitmap;
        }

        public static Bitmap colorizeBitmap(Bitmap img, Color color)
        {
            Bitmap bitmap = new Bitmap(img.Width, img.Height);

            for(int i=0;i<bitmap.Width;i++)
                for(int j=0;j<bitmap.Height;j++)
                {
                    Color c = img.GetPixel(i, j);
                    bitmap.SetPixel(i, j, Color.FromArgb(c.A, (int)(c.R + (1 - c.R / 255.0) * color.R), (int)(c.G + (1 - c.G / 255.0) * color.G), (int)(c.B + (1 - c.B / 255.0) * color.B)));
                }

            return bitmap;
        }
    }
}
