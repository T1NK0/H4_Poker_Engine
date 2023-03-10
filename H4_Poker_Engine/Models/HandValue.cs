namespace H4_Poker_Engine.Models
{
    public enum HandRank
    {
        NONE,
        PAIR,
        TWOPAIR,
        THREEKIND,
        STRAIGHT,
        FLUSH,
        FULLHOUSE,
        FOURKIND,
        STRAIGHTFLUSH,
        ROYALFLUSH
    };

    /// <summary>
    /// An object that holds the most relevant information about a players hand
    /// </summary>
    public class HandValue
    {
        private HandRank handRank;
        private Card handRankCardType;
        private Card highCard;

        public HandValue(Card highCard, Card handRankCardType, HandRank handRank)
        {
            HighCard = highCard;
            HandRankCardType = handRankCardType;
            HandRank = handRank;
        }

        public Card HighCard
        {
            get { return highCard; }
            set { highCard = value; }
        }

        public Card HandRankCardType
        {
            get { return handRankCardType; }
            set { handRankCardType = value; }
        }
        public HandRank HandRank
        {
            get { return handRank; }
            set { handRank = value; }
        }
    }
}
