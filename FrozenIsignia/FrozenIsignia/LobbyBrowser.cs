using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FrozenIsigniaClasses;

namespace FrozenIsignia
{
    public class LobbyBrowser : NetworkControl
    {
        private List<LobbyInfo> games = new List<LobbyInfo>();
        private int selection = 0;
        private Font font = new Font("Arial", 16);

        public LobbyBrowser(NetworkHandler network) : base(network)
        {
            this.BackColor = Color.Blue;
            network.send("GAMES");
        }

        public override void receive(String[] msg)
        {
            switch (msg[0])
            {
                case "GAME":
                    games.Add(new LobbyInfo(int.Parse(msg[1]), int.Parse(msg[2])));
                    Invalidate();
                    break;
                case "JOIN_SUCCESS":
                    joinLobby();
                    break;
            }
        }

        private void joinLobby()
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(joinLobby));
            else
                replaceControl(new Lobby(network));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Escape:
                    replaceControl(new Title(network));
                    break;
                case Keys.R:
                    games.Clear();
                    selection = 0;
                    network.send("GAMES");
                    break;
                case Keys.W:
                case Keys.Up:
                    if (games.Count > 0)
                        if (--selection < 0)
                            selection = games.Count - 1;
                    break;
                case Keys.S:
                case Keys.Down:
                    if (games.Count > 0)
                        selection = (selection + 1) % games.Count;
                    break;
                case Keys.Enter:
                    if(games.Count > 0)
                        network.send("JOIN " + games[selection].id);
                    break;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            String lobbies = "Games:\n";
            for (int i = 0; i < games.Count; i++)
            {
                if (i == selection)
                    lobbies += ">";
                lobbies += "Lobby " + games[i].id;
                if (i == selection)
                    lobbies += "<";
                lobbies += "\nPlayers: " + games[i].numPlayers + "\n\n";
            }
            g.DrawString(lobbies, font, Brushes.White, 0, 0);
        }
    }
}
