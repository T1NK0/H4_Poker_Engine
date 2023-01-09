using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.Interfaces
{
    public interface IHandEvaluator
    {
        HandValue GetHandValue(List<Card> hand);
    }
}