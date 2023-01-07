using Microsoft.AspNetCore.SignalR;

namespace H4_Poker_Engine.Hubs
{
    public class BasePokerHub : Hub
    {
        public event Action<string, string, string>? NewPlayerConnectedEvent;
        public event Action<string, string, string>? PlayerHasDisconnectedEvent;
        public event Action<string, string, string>? PlayerIsReadyEvent;

        public async Task PlayerConnected(string user, string message, string clientId)
        {
            NewPlayerConnectedEvent?.Invoke(user, message, clientId);
            Console.WriteLine($"player: {user} connected to hub with clientId {clientId}");
        }

        public async Task PlayerDisconnected(string user, string message, string clientId)
        {
            PlayerHasDisconnectedEvent?.Invoke(user, message, clientId);
            Console.WriteLine($"player: {user} connected to hub with clientId {clientId}");
        }

        public async Task PlayerIsReady(string user, string message, string clientId)
        {
            PlayerIsReadyEvent?.Invoke(user, message, clientId);
            Console.WriteLine($"user: {user} is ready to play");
        }
    }
}
