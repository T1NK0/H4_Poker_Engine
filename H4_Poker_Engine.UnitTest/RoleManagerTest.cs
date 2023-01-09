using H4_Poker_Engine.Models;
using H4_Poker_Engine.PokerLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H4_Poker_Engine.UnitTest
{
    internal class RoleManagerTest
    {

        [Test]
        public void MoveRolesCorrectly_WhenGameBegins_MoveRoles()
        {
            //arrange 
            RoleManager roleManager = new RoleManager();
            List<Player> players = GetPlayers(5);
            players[2].Role = Role.SMALL_BLIND;
            players[3].Role = Role.BIG_BLIND;

            //act 
            roleManager.MoveRoles(players, 5, 10);
            //assert
            Assert.True(players[3].Role == Role.SMALL_BLIND);
            Assert.True(players[4].Role == Role.BIG_BLIND);
        }

        [Test]
        public void MoveRolesCorrectly_WhenPlayerHasNoMoney_MoveRoles()
        {
            //arrange 
            RoleManager roleManager = new RoleManager();
            List<Player> players = GetPlayers(5);
            players[2].Role = Role.SMALL_BLIND;
            players[3].Role = Role.BIG_BLIND;
            players[4].Money = 2;

            //act 
            roleManager.MoveRoles(players, 5, 10);
            //assert
            Assert.True(players[3].Role == Role.SMALL_BLIND);
            Assert.True(players[0].Role == Role.BIG_BLIND);
            Assert.True(!players[4].Active);
        }

        private List<Player> GetPlayers(int amountOfPlayers)
        {
            List<Player> players = new List<Player>();
            for (int i = 0; i < amountOfPlayers; i++)
            {
                players.Add(new Player()
                {
                    Money = 200,
                    Active = true,
                    Role = Role.NONE,
                });
            }
            return players;
        }
    }
}
