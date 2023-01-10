using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace H4_Poker_Engine.Hubs
{
    public class BasePokerHub : Hub
    {
        public event Action<string, string>? NewPlayerConnectedEvent;
        public event Action<string, string>? PlayerHasDisconnectedEvent;
        public event Action<string, string>? PlayerIsReadyEvent;
        public event Action<string, string, int, string>? PlayerMadeActionEvent;

        [Authorize]
        public async Task PlayerConnected(string user, string message, string clientId)
        {
            NewPlayerConnectedEvent?.Invoke(user, clientId);
        }

        [Authorize]
        public async Task PlayerDisconnected(string user, string clientId)
        {
            PlayerHasDisconnectedEvent?.Invoke(user, clientId);
        }

        [Authorize]
        public async Task PlayerIsReady(string user, string clientId)
        {
            PlayerIsReadyEvent?.Invoke(user, clientId);
        }

        [Authorize]
        public async Task PlayerMove(string user, string message, int amount, string clientId)
        {
            PlayerMadeActionEvent?.Invoke(user, message, amount, clientId);
        }
    }
}
