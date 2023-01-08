using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace H4_Poker_Engine.Hubs
{
    public class BasePokerHub : Hub
    {
        public event Action<string, string, string>? NewPlayerConnectedEvent;
        public event Action<string, string, string>? PlayerHasDisconnectedEvent;
        public event Action<string, string, string>? PlayerIsReadyEvent;

        [Authorize]
        public async Task PlayerConnected(string user, string message, string clientId)
        {
            NewPlayerConnectedEvent?.Invoke(user, message, clientId);
        }

        [Authorize]
        public async Task PlayerDisconnected(string user, string message, string clientId)
        {
            PlayerHasDisconnectedEvent?.Invoke(user, message, clientId);
        }

        [Authorize]
        public async Task PlayerIsReady(string user, string message, string clientId)
        {
            PlayerIsReadyEvent?.Invoke(user, message, clientId);
        }
    }
}
