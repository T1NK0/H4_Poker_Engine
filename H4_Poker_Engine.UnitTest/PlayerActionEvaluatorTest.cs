using H4_Poker_Engine.Models;
using H4_Poker_Engine.PokerLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H4_Poker_Engine.UnitTest
{
    internal class PlayerActionEvaluatorTest
    {
        [Test]
        public void GetCorrectActions_WhenFirstPlayerFirstRound_GetValidActions()
        {
            //arrange 
            PlayerActionEvaluator evaluator = new PlayerActionEvaluator();
            Player player = GetPlayer(200);
            PotManager potManager = new PotManager();
            potManager.CurrentCallAmount = potManager.Big_Blind;
            potManager.TotalPotAmount += potManager.Big_Blind + potManager.Small_Blind;
            Dictionary<string, bool> expected = new Dictionary<string, bool>
            {
                { "call", true },
                { "raise", true },
                { "check", false },
                { "fold", true }
            };

            //act 
            Dictionary<string, bool> actual = evaluator.GetValidActions(player, potManager, false);

            //assert
            for (int i = 0; i < actual.Count; i++)
            {
                Assert.True(expected.ElementAt(i).Value == actual.ElementAt(i).Value);
            }
        }

        [Test]
        public void GetCorrectActions_WhenPlayerBigBlind_GetValidActions()
        {
            //arrange 
            PlayerActionEvaluator evaluator = new PlayerActionEvaluator();
            Player player = GetPlayer(200);
            PotManager potManager = new PotManager();
            player.CurrentBetInRound = potManager.Big_Blind;
            potManager.CurrentCallAmount = potManager.Big_Blind;
            potManager.TotalPotAmount += potManager.Big_Blind + potManager.Small_Blind;
            Dictionary<string, bool> expected = new Dictionary<string, bool>
            {
                { "call", false },
                { "raise", true },
                { "check", true },
                { "fold", true }
            };

            //act 
            Dictionary<string, bool> actual = evaluator.GetValidActions(player, potManager, false);

            //assert
            for (int i = 0; i < actual.Count; i++)
            {
                Assert.True(expected.ElementAt(i).Value == actual.ElementAt(i).Value);
            }
        }



        private Player GetPlayer(int moneyPlayerHolds)
        {
            return new Player() { Active = true, Money = moneyPlayerHolds };
        }
    }
}
