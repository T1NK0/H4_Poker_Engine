using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H4_Poker_Engine.Poco;

namespace H4_Poker_Engine
{
    public abstract class BaseRuleSet
    {
        #region Fields
        private int minimumPlayers;
        private int maximumPlayers;
        #endregion


        


        public abstract void RunPokerGame();

        protected virtual void AssignRoles()
        {

        }
        protected abstract void DealCards();
        protected abstract void BettingRound();
        protected virtual Player DetermineWinner()
        {

        }

        #region Properties
        public int MinimumPlayers
        {
            get { return minimumPlayers; }
            private set { minimumPlayers = value; }
        }
        public int MaximumPlayers
        {
            get { return maximumPlayers; }
            protected set { maximumPlayers = value; }
        }
        #endregion
    }
}
