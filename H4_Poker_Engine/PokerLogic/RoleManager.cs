using H4_Poker_Engine.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace H4_Poker_Engine.PokerLogic
{
    public class RoleManager
    {
        private void MoveRole(List<Player> players, Role payingRole, int amountToPay)
        {
            //loop through players until you find a suitable person for the role
            int indexOffset = 0;
            int currentRoleHolder = players.IndexOf(players.Find(p => p.Role == payingRole));

            if (currentRoleHolder == -1)
            {
                Role nextRole = Role.NONE;
                switch (payingRole)
                {
                    case Role.BIG_BLIND:
                        nextRole = Role.SMALL_BLIND;
                        break;
                    case Role.SMALL_BLIND:
                        nextRole = Role.BIG_BLIND;
                        break;
                }
                int startingIndex = players.IndexOf(players.Find(p => p.Role == nextRole));
                for (int i = 0; i < players.Count; i++)
                {
                    int index = (startingIndex + i) % players.Count;
                    if (players[index].Money >= amountToPay && players[index].Active)
                    {
                        players[index].Role = nextRole;
                    }
                }
            }

            for (int i = 1; i < players.Count; i++)
            {
                indexOffset = (currentRoleHolder + i) % players.Count;

                if (players[indexOffset].Money >= amountToPay && players[indexOffset].Active)
                {
                    players[indexOffset].Role = payingRole;
                    players[currentRoleHolder].Role = Role.NONE;
                    i = players.Count;
                }
                else
                {
                    players[indexOffset].Active = false;
                }
            }
        }

        public void MoveRoles(List<Player> players, int smallBlind, int bigBlind)
        {
            MoveRole(players, Role.BIG_BLIND, bigBlind);
            MoveRole(players, Role.SMALL_BLIND, smallBlind);
        }
    }
}
