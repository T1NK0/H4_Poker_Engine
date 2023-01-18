using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.Interfaces
{
    public interface IDeckFactory
    {
        /// <summary>
        /// Method for creating a new List of cards
        /// </summary>
        /// <returns>Returns a new deck of cards without jokers</returns>
        List<Card> GetNewDeck();
    }
}