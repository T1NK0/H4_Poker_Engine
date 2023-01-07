using Microsoft.AspNetCore.SignalR;

namespace H4_Poker_Engine.Hubs
{
    public class BasePokerHub2 : Hub
    {
        //public delegate void OnMessageEvent(string user, string message, string clientId);
        //public event OnMessageEvent? NewPlayer;
        public event Action<string, string, string> NewPlayer;

        //public Action<string, string, string> NewPlayer2;

        public async Task PlayerConnected(string user, string message, string clientId)
        {
            //OnMessage?.Invoke(user, message);
            NewPlayer?.Invoke(user, message, clientId);
            //NewPlayer2?.Invoke(user, message, clientId);
            //await Clients.Client(clientId).SendAsync("ReceiveMessage", user, message);
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
            Console.WriteLine($"From hub2: user: {user}, sent: {message}, clientId: {clientId}");
        }
    }
}
