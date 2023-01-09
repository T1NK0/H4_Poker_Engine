using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.PokerLogic
{
    public class RoleManager
    {
        private int _currentPlayerIndex = 0;
        public void MoveRoles(List<Player> players)
        {
            // Increment the current player index and wrap around if necessary
            _currentPlayerIndex = (_currentPlayerIndex + 1) % players.Count;

            // Assign the new roles to the players
            players[_currentPlayerIndex].Role = Role.DEALER;
            players[(_currentPlayerIndex + 1) % players.Count].Role = Role.BIG_BLIND;
            players[(_currentPlayerIndex + 2) % players.Count].Role = Role.SMALL_BLIND;

            // Clear the roles of the other players
            for (int i = 3; i < players.Count; i++)
            {
                players[(_currentPlayerIndex + i) % players.Count].Role = Role.NONE;
            }
        }
    }
}
