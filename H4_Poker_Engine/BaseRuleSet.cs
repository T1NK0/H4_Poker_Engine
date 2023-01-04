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
        protected abstract void AssignRoles(List<Player> players);
        protected abstract void DealCards();
        protected abstract void BettingRound();

        //override this and also take note of community cards, if playing texas hold em
        protected virtual Player DetermineWinner(List<Player> players)
        {

            return players[0];
        }

        protected virtual int GetHandValue(List<Card> hand)
        {
            List<Card> cards = hand;

            cards.OrderByDescending(card => card.Rank).ThenBy(card => card.Suit);

            if (HasFlush(cards))
            {

            }
            if (HasStraight(cards)) 
            {

            }
        }

        private bool HasFlush(List<Card> cards)
        {
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                if (cards.FindAll(card => card.Suit == suit).Count >= 5)
                    return true;
            }
            return false;
        }

        private bool HasStraight(List<Card> cards)
        {
            for (int i = 0; i < cards.Count - 4; i++)
            {
                bool isStraight = true;
                for (int j = i; j < i + 5; j++)
                {
                    if (cards[j + 1].Rank != cards[j].Rank + 1)
                    {
                        isStraight = false;
                        break;
                    }
                }

                if (isStraight)
                {
                    return true;
                }
            }
            return false;
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
