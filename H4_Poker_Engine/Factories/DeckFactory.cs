using H4_Poker_Engine.Interfaces;
using H4_Poker_Engine.Models;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace H4_Poker_Engine.Factories
{
    public class DeckFactory : IDeckFactory
    {
        public List<Card> GetNewDeck()
        {
            List<Card> cards = new List<Card>();

            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    cards.Add(new Card(rank, suit));
                }
            }
            ShuffleDeck(cards);
            return cards;
        }

        /// <summary>
        /// Shuffles a deck of cards using RandomNumberGeneration
        /// </summary>
        /// <param name="deck">The deck of cards to shuffle</param>
        private void ShuffleDeck(List<Card> deck)
        {
            var rng = RandomNumberGenerator.Create();
            int n = deck.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do rng.GetBytes(box);
                while (!(box[0] < n * (byte.MaxValue / n)));
                int k = box[0] % n;
                n--;
                Card value = deck[k];
                deck[k] = deck[n];
                deck[n] = value;
            }
        }
    }
}
