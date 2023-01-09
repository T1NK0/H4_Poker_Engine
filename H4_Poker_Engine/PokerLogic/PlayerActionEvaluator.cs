using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.PokerLogic
{
    public class PlayerActionEvaluator
    {
        public Dictionary<string, bool> GetValidActions(Player player, PotManager potManager, bool hasRaised)
        {
            Dictionary<string, bool> validActions = new Dictionary<string, bool>
            {
                { "call", CanCall(player, potManager) },
                { "raise", CanRaise(player, potManager) },
                { "check", CanCheck(player, hasRaised) },
                { "Fold", true }
            };

            return validActions;
        }

        private bool CanCall(Player player, PotManager potManager)
        {
            return true;
        }
        private bool CanRaise(Player player, PotManager potManager)
        {
            return true;
        }
        private bool CanCheck(Player player, bool hasRaised)
        {
            return true;
        }

    }
}
