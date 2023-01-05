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

            List<KeyValuePair<Player, HandValue>> playerValues = new List<KeyValuePair<Player, HandValue>>();

            for (int i = 0; i < players.Count; i++)
            {
                HandValue value = _handEvaluator.GetHandValue(players[0].CardHand);
                KeyValuePair<Player, HandValue> kv = new KeyValuePair<Player, HandValue>(players[0], value);
                playerValues.Add(kv);
            }

            playerValues.OrderBy(kv => kv.Value.HandRank).ToList();
            List<KeyValuePair<Player, HandValue>> highestHandRankplayers = playerValues.Where(kv => kv.Value.HandRank == playerValues[0].Value.HandRank).ToList();
            if (highestHandRankplayers.Count > 1)
            {
                //At least 2 players have the same HandRank
                //Compare all players' highest card and determine winner
                return playerValues[0].Key;
            }
            else
            {
                return playerValues[0].Key;
            }
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
