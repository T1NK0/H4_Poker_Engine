using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H4_Poker_Engine.Models
{
    //TODO give player turkey coins
    public class Player
    {
        private string _username;
        private string _clientId;
        private bool _active;
        private int _money;
        private int _currentBetInRound;
        private List<Card> _cardHand;

        public Player()
        {
            _cardHand = new List<Card>();
        }



        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }
        public bool Active
        {
            get { return _active; }
            set { _active = value; }
        }
        public int Money
        {
            get { return _money; }
            set { _money = value; }
        }
        public int CurrentBetInRound
        {
            get { return _currentBetInRound; }
            set { _currentBetInRound = value; }
        }
        public List<Card> CardHand
        { 
            get { return _cardHand; } 
            set { _cardHand = value; } 
        }

    }
}
