using H4_Poker_Engine.Interfaces;
using H4_Poker_Engine.Models;

namespace H4_Poker_Engine.PokerLogic
{
    public class TexasHoldEmRules : BaseRuleSet
    {
        public TexasHoldEmRules(IHandEvaluator evaluator) : base(evaluator)
        {
        }
    }
}
