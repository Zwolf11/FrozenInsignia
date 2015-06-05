using System;
using System.Collections.Generic;
using FrozenIsigniaClasses;

namespace FrozenIsignia
{
    public class LobbyInfo
    {
        public int id;
        public int numPlayers;

        public LobbyInfo(int id, int numPlayers)
        {
            this.id = id;
            this.numPlayers = numPlayers;
        }
    }
}
