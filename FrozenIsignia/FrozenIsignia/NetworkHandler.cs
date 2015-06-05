using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using FrozenIsigniaClasses;

namespace FrozenIsignia
{
    public class NetworkHandler
    {
        public int id = -1;
        private Socket socket;
        private Thread listener;
        public NetworkControl control = null;

        public NetworkHandler()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 63 }), 1211));

            Console.WriteLine(socket.Connected ? "Successfully connected" : "Connection failed");

            listener = new Thread(new ThreadStart(listen));
            listener.Start();

            send("NAME " + Properties.Settings.Default.Name);
        }

        public void close()
        {
            listener.Abort();
            socket.Close();
            Console.WriteLine("Closed connection");
        }

        private void listen()
        {
            while (true)
            {
                byte[] data = new byte[256];

                for (int i = 0; i < data.Length; i++)
                    data[i] = 32;

                try
                {
                    socket.Receive(data);
                    String[] msgs = Encoding.Default.GetString(data).Trim().Split('|');
                    for (int i = 0; i < msgs.Length - 1; i++)
                    {
                        String msg = msgs[i];
                        Console.WriteLine("Received Msg=" + msg);
                        String[] split = msg.Split();

                        if (split[0] == "ACCEPT")
                            id = int.Parse(split[1]);
                        else
                            control.receive(split);
                    }
                }
                catch (SocketException) { }
            }
        }

        public void send(String msg)
        {
            byte[] byteMessage = Encoding.Default.GetBytes(msg + '|');
            socket.Send(byteMessage);
            Console.WriteLine("Sent Msg=" + msg);
        }
    }
}
