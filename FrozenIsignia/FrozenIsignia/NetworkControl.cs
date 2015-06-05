using System;
using System.Windows.Forms;

namespace FrozenIsignia
{
    public abstract class NetworkControl : Control
    {
        protected NetworkHandler network;

        public NetworkControl(NetworkHandler network)
        {
            this.Dock = DockStyle.Fill;
            this.DoubleBuffered = true;
            this.network = network;
            network.control = this;
        }

        protected void replaceControl(NetworkControl control)
        {
            FindForm().Controls.Add(control);
            Dispose();
        }

        protected void closeProgram()
        {
            network.close();
            Application.Exit();
        }

        public abstract void receive(String[] msg);

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
        }
    }
}
