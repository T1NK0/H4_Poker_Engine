using H4_Poker_Engine.Models;
using H4_Poker_Engine.PokerLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H4_Poker_Engine.UnitTest
{
    public class BaseRuleSetTest
    {
        [Test]
        public void GetCorrectWinner_WhenGameIsOver_DetermineWinner()
        {
            //arrange 
            BaseRuleSet ruleSet = new TexasHoldEmRules(new HandEvaluator());
            List<Player> players = new List<Player>();

            Player p1 = new Player();
            p1.Username = "player 1";
            p1.CardHand = new List<Card>();
            Card card1 = new Card(Rank.JACK, Suit.CLUBS); 
            Card card2 = new Card(Rank.JACK, Suit.HEARTS); 
            p1.CardHand.Add(card1);
            p1.CardHand.Add(card2);

            Player p2 = new Player();
            p2.Username = "player 2";
            p2.CardHand = new List<Card>();
            Card card3 = new Card(Rank.THREE, Suit.CLUBS); 
            Card card4 = new Card(Rank.SEVEN, Suit.DIAMONDS);
            p2.CardHand.Add(card3);
            p2.CardHand.Add(card4);

            Card communityCard1 = new Card(Rank.FIVE, Suit.HEARTS);
            Card communityCard2 = new Card(Rank.SIX, Suit.DIAMONDS);
            Card communityCard3 = new Card(Rank.TEN, Suit.SPADES);
            Card communityCard4 = new Card(Rank.EIGHT, Suit.HEARTS);
            Card communityCard5 = new Card(Rank.TWO, Suit.SPADES);

            p1.CardHand.Add(communityCard1);
            p1.CardHand.Add(communityCard2);
            p1.CardHand.Add(communityCard3);
            p1.CardHand.Add(communityCard4);
            p1.CardHand.Add(communityCard5);

            p2.CardHand.Add(communityCard1);
            p2.CardHand.Add(communityCard2);
            p2.CardHand.Add(communityCard3);
            p2.CardHand.Add(communityCard4);
            p2.CardHand.Add(communityCard5);

            players.Add(p1);
            players.Add(p2);

            List<Player> expected = new List<Player>();
            expected.Add(p1);

            //act 
            List<Player> actual = ruleSet.DetermineWinner(players);

            //assert
            Assert.True(actual[0] == expected[0]);
        }
    }
}
