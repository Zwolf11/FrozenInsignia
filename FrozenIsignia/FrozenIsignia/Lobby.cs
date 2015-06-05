using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FrozenIsigniaClasses;

namespace FrozenIsignia
{
    public class Lobby : NetworkControl
    {
        private int hostID = -1;
        private Dictionary<int, Player> players = new Dictionary<int, Player>();
        private Font font = new Font("Arial", 16);
        private String mapName = "";

        public Lobby(NetworkHandler network) : base(network)
        {
            this.BackColor = Color.Green;
            network.send("PLAYERS");
        }

        public override void receive(String[] msg)
        {
            switch (msg[0])
            {
                case "HOST":
                    hostID = int.Parse(msg[1]);
                    goto case "ADD";
                case "ADD":
                    addPlayer(int.Parse(msg[1]), msg[2], int.Parse(msg[3]));
                    break;
                case "REMOVE":
                    removePlayer(int.Parse(msg[1]));
                    break;
                case "START":
                    mapName = msg[1];
                    startGame();
                    break;
            }
        }

        private void addPlayer(int id, String name, int team)
        {
            players.Add(id, new Player(id, name, team));
            Invalidate();
        }

        private void removePlayer(int id)
        {
            players.Remove(id);
            Invalidate();
        }

        private void startGame()
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(startGame));
            else
                replaceControl(new Game(network, mapName, players));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Escape:
                    network.send("LEAVE");
                    FindForm().Controls.Add(new LobbyBrowser(network));
                    this.Dispose();
                    break;
                case Keys.Enter:
                    if(network.id == hostID)
                        network.send("START");
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            String users = "Lobby\nHost: " + hostID + "\n\nPlayers:\n";

            foreach (Player player in players.Values)
                users += player.name + "(" + player.id + ")(" + player.team + ")\n";

            g.DrawString(users, font, Brushes.White, 0, 0);
        }
    }
}
