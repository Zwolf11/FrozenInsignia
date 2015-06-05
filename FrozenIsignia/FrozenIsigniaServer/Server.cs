using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FrozenIsigniaServer
{
    public class Server
    {
        private TcpListener socket = new TcpListener(IPAddress.Any, 1211);
        private List<User> users = new List<User>();
        private Dictionary<int, Logic> games = new Dictionary<int, Logic>();
        private int gameID = 0;

        public Server()
        {
            Console.WriteLine("Starting server on " + Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork) + ":1211");
            socket.Start();
            new Thread(new ThreadStart(waitForPlayers)).Start();
        }

        public void receive(User user, String[] msg)
        {
            if (user.gameID != -1 && !games[user.gameID].lobby)
            {
                games[user.gameID].receive(user, msg);
            }
            else
            {
                switch (msg[0])
                {
                    case "NAME":
                        setPlayerInfo(user, msg[1]);
                        break;
                    case "GAMES":
                        sendGames(user);
                        break;
                    case "CREATE":
                        createGame(user);
                        break;
                    case "JOIN":
                        joinGame(user, int.Parse(msg[1]));
                        break;
                    case "PLAYERS":
                        sendPlayers(user);
                        break;
                    case "LEAVE":
                        leaveGame(user);
                        break;
                    case "START":
                        if (user.id == games[user.gameID].host.id)
                            startGame(games[user.gameID]);
                        break;
                }
            }
        }

        private void setPlayerInfo(User user, String name)
        {
            user.name = name;
        }

        private void sendGames(User user)
        {
            foreach (Logic game in games.Values)
                if (game.lobby)
                    user.send("GAME " + game.id + " " + game.users.Count);
        }

        private void createGame(User user)
        {
            Logic logic = new Logic(gameID, user);
            user.gameID = gameID;
            user.team = 1;
            games.Add(gameID, logic);
            gameID++;

            user.send("CREATE_SUCCESS");
        }

        private void joinGame(User user, int id)
        {
            user.gameID = id;
            user.team = games[id].users.Count + 1;
            games[id].users.Add(user.id, user);

            foreach (User player in games[id].users.Values)
                player.send("ADD " + user.id + " " + user.name + " " + user.team);

            user.send("JOIN_SUCCESS");
        }

        private void sendPlayers(User user)
        {
            foreach (User player in games[user.gameID].users.Values)
                user.send((player.id == games[user.gameID].host.id ? "HOST " : "ADD ") + player.id + " " + player.name + " " + player.team);
        }

        private void leaveGame(User user)
        {
            games[user.gameID].users.Remove(user.id);

            foreach (User player in games[user.gameID].users.Values)
                player.send("REMOVE " + user.id);

            user.gameID = -1;
            user.team = -1;
        }

        private void startGame(Logic game)
        {
            game.lobby = false;

            foreach (User player in game.users.Values)
                player.send("START " + game.map.name);
        }

        public void removeUser(User user)
        {
            if (user.gameID != -1)
                leaveGame(user);

            users.Remove(user);
        }

        private void waitForPlayers()
        {
            int userID = 0;

            while (true)
            {
                if (socket.Pending())
                {
                    User user = new User(userID, this, socket.AcceptTcpClient());
                    users.Add(user);
                    user.send("ACCEPT " + userID);
                    userID++;
                }
            }
        }

        public static void Main(string[] args)
        {
            Server server = new Server();
        }
    }
}
