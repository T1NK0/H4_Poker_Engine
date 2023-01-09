using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.PokerLogic
{
    public class PotManager
    {
        private int _totalPotAmount;

        private int _currentRoundAmount;
        private int big_blind = 10;

        private int small_blind = 5;

        public int Small_Blind
        {
            get { return small_blind; }
            set { small_blind = value; }
        }


        public int Big_Blind
        {
            get { return big_blind; }
            set { big_blind = value; }
        }


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

        /// <summary>
        /// Takes <paramref name="potToAdd"/> from the <paramref name="playerToTakeFrom"/>
        /// </summary>
        /// <param name="potToAdd"></param>
        /// <param name="playerToTakeFrom"></param>
        public void AddToPot(int potToAdd, Player playerToTakeFrom)
        {
            playerToTakeFrom.Money -= potToAdd;
            playerToTakeFrom.CurrentBetInRound += potToAdd;
            TotalPotAmount += potToAdd;
        }

        /// <summary>
        /// Pays out the current pot to the player(s) who won the round
        /// </summary>
        /// <param name="winningPlayers"></param>
        public void PayOutPotToWinners(List<Player> winningPlayers)
        {
            if (winningPlayers.Count == 1)
            {
                winningPlayers[0].Money += TotalPotAmount;
            }
            else
            {
                int winningAmount = TotalPotAmount / winningPlayers.Count;

                for (int i = 0; i < winningPlayers.Count; i++)
                {
                    winningPlayers[i].Money += winningAmount;
                }
            }
        }
    }
}
