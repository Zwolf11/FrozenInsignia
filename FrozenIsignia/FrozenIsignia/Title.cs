using System;
using System.Drawing;
using System.Windows.Forms;
using FrozenIsigniaClasses;

namespace FrozenIsignia
{
    public class Title : NetworkControl
    {
        private int selection = 0;
        private Font titleFont = new Font("Arial", 36);
        private Font selectionFont = new Font("Arial", 16);

        private String[] options = new String[]
        {
            "Join Game",
            "Create Game",
            "Options",
            "Exit"
        };

        public Title(NetworkHandler network) : base(network) { }

        public override void receive(String[] msg)
        {
            switch(msg[0])
            {
                case "CREATE_SUCCESS":
                    createLobby();
                    break;
            }
        }

        private void createLobby()
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(createLobby));
            else
                replaceControl(new Lobby(network));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    closeProgram();
                    break;
                case Keys.W:
                case Keys.Up:
                    if (--selection < 0)
                        selection = options.Length - 1;
                    break;
                case Keys.S:
                case Keys.Down:
                    selection = (selection + 1) % options.Length;
                    break;
                case Keys.Enter:
                    if (selection == 0)
                        replaceControl(new LobbyBrowser(network));
                    else if (selection == 1)
                        network.send("CREATE");
                    else if (selection == 2)
                        replaceControl(new Options(network));
                    else if (selection == 3)
                        closeProgram();
                    break;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(System.Drawing.Color.FromArgb(20, 20, 20));

            StringFormat align = new StringFormat();
            align.Alignment = StringAlignment.Center;
            align.LineAlignment = StringAlignment.Far;

            g.DrawString("Frozen Isignia", titleFont, Brushes.White, new RectangleF(0, 0, ClientSize.Width, ClientSize.Height / 2), align);

            String selectString = "";
            for (int i = 0; i < options.Length; i++)
            {
                if (i == selection)
                    selectString += "> ";
                selectString += options[i];
                if (i == selection)
                    selectString += " <";
                if (i != options.Length - 1)
                    selectString += "\n";
            }
            align.LineAlignment = StringAlignment.Near;
            g.DrawString(selectString, selectionFont, Brushes.White, new RectangleF(0, ClientSize.Height / 2, ClientSize.Width, ClientSize.Height / 2), align);
        }
    }
}
