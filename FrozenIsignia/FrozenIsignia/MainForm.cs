using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using FrozenIsigniaClasses;

namespace FrozenIsignia
{
    public class MainForm : Form
    {
        private NetworkHandler network = new NetworkHandler();

        public MainForm()
        {
            this.Text = "Frozen Isignia";
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
            setFullscreen(Properties.Settings.Default.FullScreen);

            new Thread(() => Images.loadImgs(new Color[]{Color.Red, Color.Blue, Color.Green, Color.Gold})).Start();
            Title title = new Title(network);
            title.ClientSize = ClientSize;
            this.Controls.Add(title);
        }

        public void setFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = FormBorderStyle.None;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.ClientSize = new Size(Properties.Settings.Default.Width, Properties.Settings.Default.Height);
            }

            Properties.Settings.Default.FullScreen = fullscreen;
            Properties.Settings.Default.Save();
        }

        protected override void OnResize(EventArgs e)
        {
            Properties.Settings.Default.Width = this.ClientSize.Width;
            Properties.Settings.Default.Height = this.ClientSize.Height;
            Properties.Settings.Default.Save();

            foreach (Control control in Controls)
                control.ClientSize = this.ClientSize;

            this.Invalidate();
        }

        public static void Main()
        {
            Application.Run(new MainForm());
        }
    }
}
