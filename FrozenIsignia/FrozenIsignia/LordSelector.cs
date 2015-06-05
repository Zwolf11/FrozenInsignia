using System;
using System.Drawing;
using System.Windows.Forms;

namespace FrozenIsignia
{
    public class LordSelector : Control
    {
        private NetworkHandler network;

        public LordSelector(NetworkHandler network, int left, int top, int width, int height) : base("", left, top, width, height)
        {
            this.network = network;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.X < this.ClientSize.Width / 3)
                network.send("LORD Lord(Sword) " + Properties.Settings.Default.Squire);
            else if (e.X < 2 * this.ClientSize.Width / 3)
                network.send("LORD Lord(Axe) " + Properties.Settings.Default.Squire);
            else
                network.send("LORD Lord(Lance) " + Properties.Settings.Default.Squire);

            this.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Red, 0, 0, Width / 3, Height);
            g.DrawImage(Images.weapons[0], 0, 0, Width / 3, Height);
            g.FillRectangle(Brushes.Blue, Width / 3, 0, Width / 3, Height);
            g.DrawImage(Images.weapons[1], Width / 3, 0, Width / 3, Height);
            g.FillRectangle(Brushes.Green, 2 * Width / 3, 0, Width / 3, Height);
            g.DrawImage(Images.weapons[2], 2 * Width / 3, 0, Width / 3, Height);
        }
    }
}
