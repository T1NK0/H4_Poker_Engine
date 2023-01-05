using H4_Poker_Engine.Poco;

namespace H4_Poker_Engine
{
    public class HandEvaluator : IHandEvaluator
    {
        private const int _royalFlushBonus = 999;
        private const int _straighFlushBonus = 500;
        private const int _fourKindBonus = 250;
        private const int _fullHouseBonus = 200;
        private const int _flushBonus = 150;
        private const int _straightBonus = 100;
        private const int _threeKindBonus = 50;
        private const int _twoPairBonus = 25;
        private const int _pairBonus = 20;
        public virtual int GetHandValue(List<Card> hand)
        {
            List<Card> cards = hand;
            cards.OrderByDescending(card => card.Rank).ThenBy(card => card.Suit);

            if (HasRoyalFlush(cards))
            {
                return _royalFlushBonus;
            }
            else if (HasStraightFlush(cards))
            {
                return _straighFlushBonus;
            }
            else if (HasFourOfAKind(cards))
            {
                return _fourKindBonus;
            }
            else if (HasFullHouse(cards))
            {
                return _fullHouseBonus;
            }
            else if (HasFlush(cards))
            {
                return _flushBonus;
            }
            else if (HasStraight(cards))
            {
                return _straightBonus;
            }
            else if (HasThreeOfAKind(cards))
            {
                return _threeKindBonus;
            }
            else if (HasTwoPair(cards))
            {
                return _twoPairBonus;
            }
            else if (HasPair(cards))
            {
                return _pairBonus;
            }

            return (int)cards[0].Rank;
        }

        private HandValue HasRoyalFlush(List<Card> cards)
        {
            if (HasStraightFlush(cards))
            {
                //This could be optimized by placing it outside of our method,
                //but the chance of getting a royal flush is pretty slim, so its here for now
                List<Rank> royalFlush = new List<Rank>() { Rank.TEN, Rank.JACK, Rank.QUEEN, Rank.KING, Rank.ACE };

                //checks if all the elements in the royalFlush list are present in the cards list.
                return royalFlush.All(r => cards.Any(card => card.Rank == r));
            }
            return null;
        }
        private bool HasStraightFlush(List<Card> cards)
        {
            //no reason to check the last 4 cards, as a straight flush is never less than 5
            for (int i = 0; i < cards.Count - 4; i++)
            {
                bool isStraightFlush = true;
                for (int j = i; j < i + 5; j++)
                {
                    //Check if the next card has the same suit
                    if (cards[j + 1].Suit == cards[j].Suit)
                    {

                        if (cards[j + 1].Rank != cards[j].Rank)
                        {
                            //Check if the next card's rank, would be the next in row from our current card's rank.
                            if (cards[j + 1].Rank != cards[j].Rank + 1)
                            {
                                isStraightFlush = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        isStraightFlush = false;
                        break;
                    }
                }

                if (isStraightFlush)
                {
                    return true;
                }
            }
            return false;
        }
        private bool HasFourOfAKind(List<Card> cards)
        {
            //Uses GroupBy to group by rank, then checks if any group has count == 4
            return cards.GroupBy(card => card.Rank).Any(group => group.Count() == 4);
        }
        private bool HasFullHouse(List<Card> cards)
        {
            //Uses GroupBy to group by rank, then checks if a group has 3 and a group has 2 elements
            return cards.GroupBy(c => c.Rank).Any(g => g.Count() == 3)
                && cards.GroupBy(c => c.Rank).Any(g => g.Count() == 2);
        }

        private bool HasFlush(List<Card> cards)
        {
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                if (cards.FindAll(card => card.Suit == suit).Count >= 5)
                    return true;
            }
            return false;
        }

        private bool HasStraight(List<Card> cards)
        {
            //no reason to check the last 4 cards, as a straight is never less than 5
            for (int i = 0; i < cards.Count - 4; i++)
            {
                bool isStraight = true;
                for (int j = i; j < i + 5; j++)
                {
                    //Keep looping through, even if we have 2 of the same card
                    //example: 2, 3, 4, 4, 5, 6 would still be a straight
                    if (cards[j + 1].Rank != cards[j].Rank)
                    {
                        if (cards[j + 1].Rank != cards[j].Rank + 1)
                        {
                            isStraight = false;
                            break;
                        }
                    }
                }

                if (isStraight)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasThreeOfAKind(List<Card> cards)
        {
            //Uses GroupBy to group by rank, then checks if any group has count == 3
            return cards.GroupBy(card => card.Rank).Any(group => group.Count() == 3);
        }
        private bool HasTwoPair(List<Card> cards)
        {
            //Uses GroupBy to group by rank, then counts how many groups have 2 in them.
            return cards.GroupBy(cards => cards.Rank).Count(group => group.Count() == 2) == 2;
        }
        private bool HasPair(List<Card> cards)
        {
            //Uses GroupBy to group by rank, then checks if any group has count == 2
            return cards.GroupBy(card => card.Rank).Any(group => group.Count() == 2);
        }
    }
}
