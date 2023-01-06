using H4_Poker_Engine.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace H4_Poker_Engine.Services
{
    public class TableServiceWorker : BackgroundService
    {
        private readonly IHubContext<BasePokerHub> _hubContext;
        private string? _user;
        private string? _message;
        private List<string> _users;

        public TableServiceWorker(IHubContext<BasePokerHub> hubContext)
        {
            _hubContext = hubContext;
            if (_users == null)
            {
                _users = new List<string>();
            }
            // Subscribe to event
            BasePokerHub.OnMessage += DoSomethingAsync;
            BasePokerHub.NewPlayer += DoSomethingAsync2;
        }

        private void DoSomethingAsync2(string arg1, string arg2, string arg3)
        {
            _users.Add(arg3);
            Console.WriteLine($"number of users: {_users.Count()}");
        }

        private async void DoSomethingAsync(string user, string message)
        {
            Console.WriteLine("Enter listener method from worker!");
            _user= user;
            _message= message;
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", _user, _message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while(!stoppingToken.IsCancellationRequested)
            //{
            //    await Task.Delay(1000);
            //    //await _hubContext.Clients.All.SendAsync("ReceiveMessage", _user, _message);
            //    Console.WriteLine($"number of users: {_user.Count()}");
            //}
        }
    }
}
