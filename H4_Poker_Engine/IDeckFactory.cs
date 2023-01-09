using H4_Poker_Engine.Models;

namespace H4_Poker_Engine
{
    public interface IDeckFactory
    {
        List<Card> GetNewDeck();
    }
}