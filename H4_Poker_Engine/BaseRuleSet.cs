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
        private IHandEvaluator _handEvaluator;

        protected BaseRuleSet(IHandEvaluator evaluator)
        {
            _handEvaluator = evaluator;
        }
        #endregion

        public abstract void RunPokerGame();
        protected abstract void AssignRoles(List<Player> players);
        protected abstract void DealCards();
        protected abstract void BettingRound();

        //override this and also take note of community cards, if playing texas hold em
        protected virtual Player DetermineWinner(List<Player> players)
        {
            
            List<KeyValuePair<Player, int>> playerValues = new List<KeyValuePair<Player, int>>();

            for (int i = 0; i < players.Count; i++)
            {
                int value = _handEvaluator.GetHandValue(players[0].CardHand);
                KeyValuePair<Player, int> kv = new KeyValuePair<Player, int>(players[0], value);
                playerValues.Add(kv);
            }

            var group = playerValues.GroupBy(kv => kv.Value).First();
            if (group.Count() > 1)
            {
                //Compare all players' highest card and determine winner

            }
            return group.First().Key;
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
