namespace H4_Poker_Engine.Authentication
{
    public interface ITokenGenerator
    {
        string GenerateLoginToken();
        string GenerateUserToken(string username);
    }
}