using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H4_Poker_Engine.Poco
{
    //TODO give player turkey coins and a list of cards
    public class Player
    {
        private string username;

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        private bool active;

        public bool Active
        {
            get { return active; }
            set { active = value; }
        }


    }
}
