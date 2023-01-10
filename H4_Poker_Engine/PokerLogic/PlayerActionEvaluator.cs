﻿using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.PokerLogic
{
    public class PlayerActionEvaluator
    {
        public Dictionary<string, bool> GetValidActions(Player player, PotManager potManager, bool hasRaised)
        {
            Dictionary<string, bool> validActions = new Dictionary<string, bool>
            {
                { "call", CanCall(player, potManager, hasRaised) },
                { "raise", CanRaise(player, potManager) },
                { "check", CanCheck(player, potManager, hasRaised) },
                { "Fold", true }
            };

            return validActions;
        }

        private bool CanCall(Player player, PotManager potManager, bool hasRaised)
        {
            if (player.CurrentBetInRound < potManager.Big_Blind)
            {
                return true;
            }
            else if (hasRaised && (player.CurrentBetInRound + player.Money) >= potManager.CurrentCallAmount)
            {
                return true;
            }
            //TODO do something about "all in"
            else if (true)
            {

            }
            return false;
        }
        private bool CanRaise(Player player, PotManager potManager)
        {
            if (potManager.CurrentCallAmount < player.Money)
            {
                return true;
            }
            return false;
        }
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