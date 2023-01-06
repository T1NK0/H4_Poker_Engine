using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H4_Poker_Engine.Models
{
    public enum Rank
    {
        ACE,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        TEN,
        JACK,
        QUEEN,
        KING
    };

    public enum Suit
    {
        SPADES,
        HEARTS,
        DIAMONDS,
        CLUBS
    };

    public class Card
    {
        public Rank Rank { get; set; }
        public Suit Suit { get; set; }

        public Card(Rank rank, Suit suit)
        {
            Rank = rank;
            Suit = suit;
        }
    }
}
