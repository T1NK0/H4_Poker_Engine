using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace H4_Poker_Engine.Hubs
{
    public class BasePokerHub : Hub
    {
        public event Action<string, string, string>? NewPlayerConnectedEvent;
        public event Action<string, string, string>? PlayerHasDisconnectedEvent;
        public event Action<string, string, string>? PlayerIsReadyEvent;
        public event Action<string, string, int, string>? PlayerMadeActionEvent;

        [Authorize]
        public void PlayerConnected(string user, string message, string clientId)
        {
            NewPlayerConnectedEvent?.Invoke(user, message, clientId);
        }

        [Authorize]
        public void PlayerDisconnected(string user, string message, string clientId)
        {
            PlayerHasDisconnectedEvent?.Invoke(user, message, clientId);
        }

        [Authorize]
        public void PlayerIsReady(string user, string message, string clientId)
        {
            PlayerIsReadyEvent?.Invoke(user, message, clientId);
        }

        [Authorize]
        public void PlayerMove(string user, string message, int amount, string clientId)
        {
            PlayerMadeActionEvent?.Invoke(user, message, amount, clientId);
        }
    }
}
