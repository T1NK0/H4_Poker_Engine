using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.PokerLogic
{
    public class PlayerActionEvaluator
    {
        /// <summary>
        /// Gets a dictionary, containing all the actions and if they're available in the current context
        /// </summary>
        /// <param name="player">The player who is about to receive his available actions</param>
        /// <param name="potManager">The potmanager of the game</param>
        /// <param name="hasRaised">if a player has raised</param>
        /// <returns>Returns a dictionary of all actions and if they're plausible currently</returns>
        public Dictionary<string, bool> GetValidActions(Player player, PotManager potManager, bool hasRaised)
        {
            Dictionary<string, bool> validActions = new Dictionary<string, bool>
            {
                { "call", CanCall(player, potManager, hasRaised) },
                { "raise", CanRaise(player, potManager) },
                { "check", CanCheck(player, potManager, hasRaised) },
                { "fold", true }
            };
            return validActions;
        }

        /// <summary>
        /// Checks if a player can call
        /// </summary>
        /// <param name="player">The player who is receiving actions</param>
        /// <param name="potManager">the potmanager of the game</param>
        /// <param name="hasRaised">flag of whether a player has raised in the current round</param>
        /// <returns>True/false whether a player can call or not</returns>
        private bool CanCall(Player player, PotManager potManager, bool hasRaised)
        {
            if (potManager.CurrentCallAmount == 0)
            {
                return false;
            }
            else if (player.CurrentBetInRound < potManager.Big_Blind)
            {
                return true;
            }
            else if(player.CurrentBetInRound < potManager.CurrentCallAmount)
            {
                return true;
            }
            else if (hasRaised && (player.CurrentBetInRound + player.Money) >= potManager.CurrentCallAmount)
            {
                return true;
            }
            //TODO do something about "all in"
            return false;
        }

        /// <summary>
        /// Checks if a player can raise
        /// </summary>
        /// <param name="player">The player who is receiving actions</param>
        /// <param name="potManager">the potmanager of the game</param>
        /// <returns>True/false whether a player can raise or not</returns>
        private bool CanRaise(Player player, PotManager potManager)
        {
            if (potManager.CurrentCallAmount < player.Money)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a player can check
        /// </summary>
        /// <param name="player">The player who is receiving actions</param>
        /// <param name="potManager">the potmanager of the game</param>
        /// <param name="hasRaised">flag of whether a player has raised in the current round</param>
        /// <returns>True/false whether a player can check or not</returns>
        private bool CanCheck(Player player, PotManager potManager, bool hasRaised)
        {
            if (!hasRaised && player.CurrentBetInRound == potManager.CurrentCallAmount)
            {
                return true;
            }
            return false;
        }

    }
}
