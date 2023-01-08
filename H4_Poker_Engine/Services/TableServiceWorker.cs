using H4_Poker_Engine.Hubs;
using H4_Poker_Engine.Models;
using Microsoft.AspNetCore.SignalR;

namespace H4_Poker_Engine.Services
{
    public class TableServiceWorker : BackgroundService
    {
        private readonly IHubContext<BasePokerHub> _hubContext;
        private List<Player> _users;

        public TableServiceWorker(IHubContext<BasePokerHub> hubContext, BasePokerHub hub)
        {
            _hubContext = hubContext;

            if (_users == null)
            {
                _users = new List<Player>();
            }

            // Subscribe to event
            hub.NewPlayerConnectedEvent += AddNewPlayerToGame;
            hub.PlayerHasDisconnectedEvent += RemovePlayerFromGame;
            hub.PlayerIsReadyEvent += PlayerIsReadyToPlay;
        }


        private void AddNewPlayerToGame(string user, string message, string clientId)
        {
            var newPlayer = new Player() { Username = user, ClientId = clientId, Active = false };
            _users.Add(newPlayer);
            Console.WriteLine($"New player added, total number of users: {_users.Count()}");
        }

        private void RemovePlayerFromGame(string user, string message, string clientId)
        {
            var playerToRemove = _users.Find(player => player.ClientId == clientId);
            if (playerToRemove != null)
            {
                _users.Remove(playerToRemove);
                Console.WriteLine($"Player {user} removed from game, total number of players: {_users.Count()}");
            }
        }
        private void PlayerIsReadyToPlay(string user, string message, string clientId)
        {
            var playerToBeReady = _users.Find(player => player.ClientId == clientId);
            if (playerToBeReady != null)
            {
                playerToBeReady.Active = true;
                Console.WriteLine($"Player {user} is ready to play, status is set to {playerToBeReady.Active}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // TODO: some logic here!?
        }
    }
}
