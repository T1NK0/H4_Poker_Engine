using H4_Poker_Engine.Interfaces;
using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.PokerLogic
{
    public abstract class BaseRuleSet
    {
        #region Fields
        protected int minimumPlayers;
        protected int maximumPlayers;
        protected IHandEvaluator _handEvaluator;

        protected BaseRuleSet(IHandEvaluator evaluator)
        {
            _handEvaluator = evaluator;
        }
        #endregion

        
        /// <summary>
        /// Deal a specified amount of cards from a deck, to a list of players
        /// </summary>
        /// <param name="playersToDeal">Players to deal to</param>
        /// <param name="deck">The deck to deal from</param>
        /// <param name="amountToDeal">The amount each player will receive</param>
        public virtual void DealCards(List<Player> playersToDeal, List<Card> deck, int amountToDeal)
        {
            Console.WriteLine("------- Enters: Deal User Cards -------");
            for (int i = 0; i < playersToDeal.Count; i++)
            {
                for (int j = 0; j < amountToDeal; j++)
                {
                    playersToDeal[i].CardHand.Add(deck.First());
                    deck.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Method for determining a winner or winners
        /// </summary>
        /// <param name="players">The list of players to find a winner(s) from</param>
        /// <returns>The player(s) who won</returns>
        public virtual List<Player> DetermineWinner(List<Player> players)
        {

            List<KeyValuePair<Player, HandValue>> playerValues = new List<KeyValuePair<Player, HandValue>>();

            for (int i = 0; i < players.Count; i++)
            {
                HandValue value = _handEvaluator.GetHandValue(players[i].CardHand);
                KeyValuePair<Player, HandValue> kv = new KeyValuePair<Player, HandValue>(players[i], value);
                playerValues.Add(kv);
            }

            playerValues.OrderBy(kv => kv.Value.HandRank).ToList();
            HandValue highestValue = playerValues.First().Value;

            List<KeyValuePair<Player, HandValue>> highestHandRankPlayers =
                playerValues.Where(kv => kv.Value.HandRank == highestValue.HandRank).ToList();
            if (highestHandRankPlayers.Count > 1)
            {
                //At least 2 players have the same HandRank
                //Compare all players' highest card and determine winner
                List<Player> winningPlayers = new List<Player>();
                HandValue highestCard = null;
                for (int i = 0; i < highestHandRankPlayers.Count; i++)
                {
                    //if this is first iteration or if the [i] players rank is bigger thank the currently biggest,
                    //add him to winners and remove the old winner
                    if (highestCard == null ||
                        highestHandRankPlayers[i].Value.HandRankCardType.Rank > highestCard.HandRankCardType.Rank)
                    {
                        winningPlayers.Clear();
                        winningPlayers.Add(highestHandRankPlayers[i].Key);
                        highestCard = highestHandRankPlayers[i].Value;
                        continue;
                    }

                    //add the player to winningPlayers, if they have the same handRank
                    if (highestHandRankPlayers[i].Value.HandRankCardType.Rank == highestCard.HandRankCardType.Rank)
                    {
                        //Split the pot
                        winningPlayers.Add(highestHandRankPlayers[i].Key);
                        continue;
                    }
                }
                return winningPlayers;
            }
            else
            {
                return new List<Player>() { highestHandRankPlayers[0].Key };
            }
        }



        #region Properties
        public int MinimumPlayers
        {
            get { return minimumPlayers; }
            protected set { minimumPlayers = value; }
        }
        public int MaximumPlayers
        {
            get { return maximumPlayers; }
            protected set { maximumPlayers = value; }
        }
        #endregion
    }
}
