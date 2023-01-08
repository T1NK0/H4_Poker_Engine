namespace H4_Poker_Engine.Models
{
    public class CardModel
    {
        public string Suit { get; set; }
        public string Value { get; set; }

        public CardModel(string suit, string value)
        {
            Suit = suit;
            Value = value;
        }
    }
}
