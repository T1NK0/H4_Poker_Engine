using H4_Poker_Engine.Poco;

namespace H4_Poker_Engine
{
    public interface IHandEvaluator
    {
        int GetHandValue(List<Card> hand);
    }
}