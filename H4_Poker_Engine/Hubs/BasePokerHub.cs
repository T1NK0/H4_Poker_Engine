using Microsoft.AspNetCore.SignalR;

namespace H4_Poker_Engine.Hubs
{
    public class BasePokerHub : Hub
    {
        public delegate void OnMessageEvent(string user, string message);
        public static event OnMessageEvent? OnMessage;
        public static Action<string, string, string> NewPlayer;

        public async Task PlayerConnected(string user, string message, string clientId)
        {
            OnMessage?.Invoke(user, message);
            NewPlayer?.Invoke(user, message, clientId);
            //await Clients.Client(clientId).SendAsync("ReceiveMessage", user, message);
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
            Console.WriteLine($"user: {user}, sent: {message}, clientId: {clientId}");
        }
    }
}
