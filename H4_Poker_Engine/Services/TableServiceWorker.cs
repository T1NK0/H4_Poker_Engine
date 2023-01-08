using H4_Poker_Engine.Hubs;
using H4_Poker_Engine.Models;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;
using System.Security.Cryptography;

namespace H4_Poker_Engine.Services
{
    public class TableServiceWorker : BackgroundService
    {
        private readonly IHubContext<BasePokerHub> _hubContext;
        private List<Player> _users;
        private List<CardModel> _deck;
        private bool _isGameRunning = false;

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
            CheckIfGameCanBegin();
        }

        private void CheckIfGameCanBegin()
        {
            var numberOfPlayersReady = 0;
            foreach (var player in _users)
            {
                if (player.Active)
                {
                    numberOfPlayersReady++;
                }
            }
            if (numberOfPlayersReady >= 2 && _isGameRunning == false)
            {
                _isGameRunning = true;
                GiveCards();
            }
        }

        private async void GiveCards()
        {
            GetANewDeck();
            foreach (var player in _users)
            {
                var firstCard = _deck.FirstOrDefault();
                _deck.Remove(firstCard);
                var secondCard = _deck.FirstOrDefault();
                _deck.Remove(secondCard);
                await _hubContext.Clients.Client(player.ClientId).SendAsync("GetPlayerCards", firstCard, secondCard);
            }
            var tableFirstCard = _deck.FirstOrDefault();
            _deck.Remove(tableFirstCard);
            var tableSecondCard = _deck.FirstOrDefault();
            _deck.Remove(tableSecondCard);
            var tableTheedCard = _deck.FirstOrDefault();
            _deck.Remove(tableTheedCard);
            await _hubContext.Clients.All.SendAsync("GetTableCards", tableFirstCard, tableSecondCard, tableTheedCard);
        }

        private void GetANewDeck()
        {
            if (_deck == null)
            {
                _deck = new List<CardModel>();
            }
            else
            {
                _deck.Clear();
            }
            List<CardModel> deck = new List<CardModel>();

            string[] suits = { "hearts", "diamonds", "spades", "clubs" };
            string[] values = { "ace", "2", "3", "4", "5", "6", "7", "8", "9", "10", "jack", "queen", "king" };

            foreach (string suit in suits)
            {
                foreach (string value in values)
                {
                    deck.Add(new CardModel(suit, value));
                }
            }

            _deck = Shuffle(deck);
        }

        private List<CardModel> Shuffle(List<CardModel> deck)
        {
            var rng = RandomNumberGenerator.Create();
            int n = deck.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do rng.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                CardModel value = deck[k];
                deck[k] = deck[n];
                deck[n] = value;
            }
            return deck;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // TODO: some logic here!?
        }
    }
}
