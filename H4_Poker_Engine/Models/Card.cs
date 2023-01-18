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

    //Makes it so when you convert to json, it converts it into the name of the enum and not the int value
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
}
