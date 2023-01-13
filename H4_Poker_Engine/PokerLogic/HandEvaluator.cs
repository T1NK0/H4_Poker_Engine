using H4_Poker_Engine.Interfaces;
using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.PokerLogic
{
    public class HandEvaluator : IHandEvaluator
    {
        public virtual HandValue GetHandValue(List<Card> hand)
        {
            List<Card> cards = hand.OrderByDescending(card => card.Rank)
                .ThenBy(card => card.Suit).ToList();

            HandValue? hv = null;

            hv = HasRoyalFlush(cards);
            if (hv != null)
            {
                return hv;
            }

            hv = HasStraightFlush(cards);
            if (hv != null)
            {
                return hv;
            }

            hv = HasFourOfAKind(cards);
            if (hv != null)
            {
                return hv;
            }

            hv = HasFullHouse(cards);
            if (hv != null)
            {
                return hv;
            }

            hv = HasFlush(cards);
            if (hv != null)
            {
                return hv;
            }

            hv = HasStraight(cards);
            if (hv != null)
            {
                return hv;
            }

            hv = HasThreeOfAKind(cards);
            if (hv != null)
            {
                return hv;
            }

            hv = HasTwoPair(cards);
            if (hv != null)
            {
                return hv;
            }

            hv = HasPair(cards);
            if (hv != null)
            {
                return hv;
            }

            return new HandValue(cards[0], cards[0], HandRank.NONE);
        }

        private HandValue HasRoyalFlush(List<Card> cards)
        {
            if (HasStraightFlush(cards) != null)
            {
                //This could be optimized by placing it outside of our method,
                //but the chance of getting a royal flush is pretty slim, so its here for now
                List<Rank> royalFlush = new List<Rank>() { Rank.TEN, Rank.JACK, Rank.QUEEN, Rank.KING, Rank.ACE };

                //checks if all the elements in the royalFlush list are present in the cards list.
                if (royalFlush.All(r => cards.Any(card => card.Rank == r)))
                {
                    return new HandValue(cards.Where(c => !royalFlush.Contains(c.Rank)).OrderByDescending(c => c.Rank).First(),
                        cards.Find(card => card.Rank == Rank.KING),
                        HandRank.ROYALFLUSH);
                }
            }
            return null;
        }

        private HandValue HasStraightFlush(List<Card> cards)
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
                    List<Card> straightFlushCards = cards.Skip(i).Take(5).ToList();
                    return new HandValue(cards.First(card => !straightFlushCards.Contains(card)),
                        straightFlushCards[0],
                        HandRank.STRAIGHTFLUSH);
                }
            }
            return null;
        }
        private HandValue HasFourOfAKind(List<Card> cards)
        {
            //Uses GroupBy to group by rank, then checks if any group has count == 4
            foreach (var group in cards.GroupBy(card => card.Rank))
            {
                if (group.Count() == 4)
                {
                    return new HandValue(cards.First(card => !group.ToList().Contains(card)), group.First(), HandRank.FOURKIND);
                }
            }
            return null;
        }
        private HandValue HasFullHouse(List<Card> cards)
        {
            //Uses GroupBy to group by rank, then checks if a group has 3 and a group has 2 elements
            var groups = cards.GroupBy(card => card.Rank);
            var threeKindGroup = groups.FirstOrDefault(group => group.Count() == 3);

            if (threeKindGroup != null)
            {
                var pairGroup = groups.FirstOrDefault(group => group.Count() == 2);

                if (pairGroup != null)
                {
                    //the highcard here is the three of a kind, which will first be used to determine a winner
                    //next will be the pair in case the three of a kind is a draw
                    return new HandValue(threeKindGroup.First(), pairGroup.First(), HandRank.FULLHOUSE);
                }
            }
            return null;
        }

        private HandValue HasFlush(List<Card> cards)
        {
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                List<Card> potentialFlushCards = cards.FindAll(card => card.Suit == suit).ToList();
                if (potentialFlushCards.Count >= 5)
                {
                    return new HandValue(
                        cards.First(card => !potentialFlushCards.Contains(card)),
                        potentialFlushCards.First(),
                        HandRank.FLUSH);
                }
            }
            return null;
        }

        private HandValue HasStraight(List<Card> cards)
        {
            //no reason to check the last 4 cards, as a straight is never less than 5
            for (int i = 0; i < cards.Count - 4; i++)
            {
                bool isStraight = true;
                bool hasDuplicate = false;
                for (int j = i; j < i + 5; j++)
                {
                    if (j + 1 < cards.Count)
                    {
                        if (cards[j + 1].Rank == cards[j].Rank)
                        {
                            hasDuplicate = true;
                        }
                        else if (cards[j + 1].Rank != cards[j].Rank + 1)
                        {
                            isStraight = false;
                            break;
                        }
                    }
                }
                if (isStraight && !hasDuplicate)
                {
                    List<Card> straightCards = cards.Skip(i).Take(5).ToList();
                    return new HandValue(cards.First(card => !straightCards.Contains(card)),
                        straightCards[0],
                        HandRank.STRAIGHT);
                }
            }
            return null;
        }

        private HandValue HasThreeOfAKind(List<Card> cards)
        {
            try
            {
                //Uses GroupBy to group by rank, then checks if any group has count == 3
                List<Card> threeKindCards = cards.GroupBy(card => card.Rank).First(group => group.Count() == 3).ToList();
                if (threeKindCards != null)
                {
                    return new HandValue(cards.First(card => !threeKindCards.Contains(card)),
                        threeKindCards[0],
                        HandRank.STRAIGHT);
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        private HandValue HasTwoPair(List<Card> cards)
        {
            try
            {
                //Uses GroupBy to group by rank, then counts how many groups have 2 in them.
                var groups = cards.GroupBy(cards => cards.Rank).Where(group => group.Count() == 2).ToList();

                if (groups.Count >= 2)
                {
                    return new HandValue(groups.ElementAt(1).First(),
                       groups.First().First(),
                       HandRank.TWOPAIR);
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        private HandValue HasPair(List<Card> cards)
        {
            try
            {
                //Uses GroupBy to group by rank, then checks if any group has count == 2
                var group = cards.GroupBy(card => card.Rank).First(group => group.Count() == 2).ToList();
                if (group.Count >= 1)
                {
                    return new HandValue(cards.Where(card => !group.Contains(card)).First(),
                       group.First(),
                       HandRank.PAIR);
                }
                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
