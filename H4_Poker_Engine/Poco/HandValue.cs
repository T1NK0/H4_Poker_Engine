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
        private Rank rank;
        private Card highCard;

        public HandValue(Card highCard, Rank rank, HandRank handRank)
        {
            HighCard = highCard;
            Rank = rank;
            HandRank = handRank;
        }

        public Card HighCard
        {
            get { return highCard; }
            set { highCard = value; }
        }

        public Rank Rank
        {
            get { return rank; }
            set { rank = value; }
        }
        public HandRank HandRank
        {
            get { return handRank; }
            set { handRank = value; }
        }
    }
}
