using H4_Poker_Engine.Poco;

namespace H4_Poker_Engine
{
    public interface IHandEvaluator
    {
        HandValue GetHandValue(List<Card> hand);
    }
}