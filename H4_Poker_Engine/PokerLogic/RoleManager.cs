using H4_Poker_Engine.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace H4_Poker_Engine.PokerLogic
{
    public class RoleManager
    {
        /// <summary>
        /// Moves the big and small blind roles in a list of players
        /// </summary>
        /// <param name="players">Players to iterate through</param>
        /// <param name="smallBlind">The amount needed to receive <see cref="Role.SMALL_BLIND"/></param>
        /// <param name="bigBlind"> The amount needed to receive <see cref="Role.BIG_BLIND"/></param>
        /// <returns></returns>
        public bool MoveRoles(List<Player> players, int smallBlind, int bigBlind)
        {
            List<Player> activePlayers = GetActivePlayers(players);

            if (IsSuitableRoleHolder(activePlayers, Role.SMALL_BLIND, smallBlind))
            {
                MoveSmallBlind(activePlayers, smallBlind);

                if (IsSuitableRoleHolder(activePlayers, Role.BIG_BLIND, bigBlind))
                {
                    MoveBigBlind(activePlayers, bigBlind);
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Gets all players that are currently active in the game
        /// </summary>
        /// <param name="players">The list of players to check through</param>
        /// <returns>Returns a list of all active players</returns>
        private List<Player> GetActivePlayers(List<Player> players)
        {
            return players.Where(p => p.Active).ToList();
        }


        /// <summary>
        /// Checks in a list of players if there is a suitable player for a specified role
        /// </summary>
        /// <param name="players">The list of players to check</param>
        /// <param name="roleToCheck">The role to check on</param>
        /// <param name="amountToPay">the amount the player needs to pay to receive the role</param>
        /// <returns></returns>
        private bool IsSuitableRoleHolder(List<Player> players, Role roleToCheck, int amountToPay)
        {
            if (players.Any(p => p.Money >= amountToPay && p.Role != roleToCheck))
            {
                return true;
            }

            return false;
        }

        private void MoveSmallBlind(List<Player> players, int amountToPay)
        {
            int currentRoleIndex = players.IndexOf(players.Find(p => p.Role == Role.SMALL_BLIND));

            for (int i = 0; i < players.Count; i++)
            {
                int index = (i + currentRoleIndex) % players.Count;
                if (players[index].Money >= amountToPay && players[index].Role != Role.SMALL_BLIND)
                {
                    //Set holder perhaps
                    players[index].Role = Role.SMALL_BLIND;
                    players[currentRoleIndex].Role = Role.NONE;
                    i = players.Count;
                }
            }
        }

        private void MoveBigBlind(List<Player> players, int amountToPay)
        {
            int currentRoleIndex = players.IndexOf(players.Find(p => p.Role == Role.BIG_BLIND));

            if (currentRoleIndex == -1)
            {
                int smallBlindIndex = players.IndexOf(players.Find(p => p.Role == Role.SMALL_BLIND));
                for (int i = 0; i < players.Count; i++)
                {
                    int index = (i + smallBlindIndex) % players.Count;
                    if (players[index].Role != Role.SMALL_BLIND && players[index].Money >= amountToPay)
                    {
                        players[index].Role = Role.BIG_BLIND;
                        i = players.Count;
                    }
                }

            }
            else
            {
                for (int i = 0; i < players.Count; i++)
                {
                    int index = (i + currentRoleIndex) % players.Count;
                    if (players[index].Role != Role.BIG_BLIND && players[index].Money >= amountToPay)
                    {
                        players[index].Role = Role.BIG_BLIND;
                        players[currentRoleIndex].Role = Role.NONE;
                        i = players.Count;
                    }
                }
            }
        }
    }
}
