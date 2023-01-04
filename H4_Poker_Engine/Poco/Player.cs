using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H4_Poker_Engine.Poco
{
    //TODO give player turkey coins
    public class Player
    {
        private string username;
        private bool active;
        private List<Card> cardHand;

        public Player()
        {
            cardHand = new List<Card>();
        }



        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        
    }
}
