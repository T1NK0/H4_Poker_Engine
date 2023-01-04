using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H4_Poker_Engine.Poco;

namespace H4_Poker_Engine
{
    public abstract class BaseRuleSet
    {
        private int minimumPlayers;

        public int MinimumPlayers
        {
            get { return minimumPlayers; }
            private set { minimumPlayers = value; }
        }

        private int maximumPlayers;

        public int MaximumPlayers
        {
            get { return maximumPlayers; }
            protected set { maximumPlayers = value; }
        }


        public abstract void RunPokerGame();

        protected virtual void AssignRoles()
        {

        }

        protected abstract void DealCards();
        protected abstract void BettingRound();
        protected virtual Player DetermineWinner()
        {

        }
    }
}
