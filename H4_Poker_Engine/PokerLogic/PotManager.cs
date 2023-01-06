using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.PokerLogic
{
    public class PotManager
    {
        private int _totalPotAmount;

        private int _currentRoundAmount;

        public int CurrentRoundAmount
        {
            get { return _currentRoundAmount; }
            set { _currentRoundAmount = value; }
        }

        public int TotalPotAmount
        {
            get { return _totalPotAmount; }
            set { _totalPotAmount = value; }
        }

        public void AddToPot(int potToAdd)
        {
            TotalPotAmount += potToAdd;
        }

        /// <summary>
        /// Pays out the current pot to the player(s) who won the round
        /// </summary>
        /// <param name="winningPlayers"></param>
        public void PayOutPotToWinners(List<Player> winningPlayers)
        {

        }
    }
}
