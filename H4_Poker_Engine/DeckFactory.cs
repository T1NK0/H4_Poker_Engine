using H4_Poker_Engine.Models;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace H4_Poker_Engine
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

            return cards;
        }

        private void ShuffleDeck(List<Card> deck)
        {
            var rng = RandomNumberGenerator.Create();
            int n = deck.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do rng.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                CardModel value = deck[k];
                deck[k] = deck[n];
                deck[n] = value;
            }
        }
    }
}
