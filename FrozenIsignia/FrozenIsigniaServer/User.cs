using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using FrozenIsigniaClasses;

namespace FrozenIsigniaServer
{
    public class User : Player
    {
        private Server server;
        private TcpClient client;
        public int gameID = -1;

        public User(int id, Server server, TcpClient client) : base(id)
        {
            this.server = server;
            this.client = client;
            new Thread(new ThreadStart(listen)).Start();
        }

        private void listen()
        {
            while (client.Client.Connected)
            {
                byte[] data = new byte[256];

                for (int i = 0; i < data.Length; i++)
                    data[i] = 32;

                try
                {
                    client.Client.Receive(data);
                    String[] msgs = Encoding.Default.GetString(data).Trim().Split('|');
                    for (int i = 0; i < msgs.Length - 1; i++)
                    {
                        String msg = msgs[i];
                        Console.WriteLine("Received User=" + id + " Msg=" + msg);
                        server.receive(this, msg.Split());
                    }
                }
                catch (SocketException) { }
            }

            client.Close();
            Console.WriteLine("Disconnected User=" + id);
            server.removeUser(this);
        }

        public void send(String msg)
        {
            byte[] byteMessage = Encoding.Default.GetBytes(msg + '|');
            client.Client.Send(byteMessage);
            Console.WriteLine("Sent User=" + id + " Msg=" + msg);
        }
    }
}
