using System.Text.Json.Serialization;

namespace H4_Poker_Engine.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
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

    [JsonConverter(typeof(JsonStringEnumConverter))]
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

    //public class CardModel
    //{
    //    public string Suit { get; set; }
    //    public string Value { get; set; }

    //    //public CardModel(string suit, string value)
    //    //{
    //    //    Suit = suit;
    //    //    Value = value;
    //    //}
    //    public CardModel(Card card)
    //    {
    //        Suit = card.Suit.ToString();
    //        Value = card.Rank.ToString();
    //    }
    //}
}
