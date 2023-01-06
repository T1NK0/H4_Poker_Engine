namespace H4_Poker_Engine.Poco
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
