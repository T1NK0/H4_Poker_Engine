﻿using H4_Poker_Engine.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace H4_Poker_Engine.Services
{
    public class TableServiceWorker : BackgroundService
    {
        private readonly IHubContext<BasePokerHub> _hubContext;
        private List<string> _users;

        public TableServiceWorker(IHubContext<BasePokerHub> hubContext, BasePokerHub hub)
        {
            _hubContext = hubContext;

            if (_users == null)
            {
                _users = new List<string>();
            }
            // Subscribe to event
            //hub.OnMessage += DoSomethingAsync;
            hub.NewPlayerConnectedEvent += AddNewPlayerToGame;
            hub.PlayerHasDisconnectedEvent += RemovePlayerFromGame;

            //hub.NewPlayer2 += DoSomethingAsync;


        }

        private void AddNewPlayerToGame(string user, string message, string clientId)
        {
            _users.Add(clientId);
            Console.WriteLine($"New player added, total number of users: {_users.Count()}");
        }

        private  void RemovePlayerFromGame(string user, string message, string clientId)
        {
            _users.Remove(clientId);
            Console.WriteLine($"Player {user} removed from game, total number of players: {_users.Count()}");
            
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
