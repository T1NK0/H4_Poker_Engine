using H4_Poker_Engine.Hubs;
using H4_Poker_Engine.Interfaces;
using H4_Poker_Engine.Models;
using H4_Poker_Engine.PokerLogic;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;
using System.Security.Cryptography;

namespace H4_Poker_Engine.Services
{
    public class TableServiceWorker : BackgroundService
    {
        private readonly IHubContext<BasePokerHub> _hubContext;
        private IDeckFactory _deckFactory;
        private List<Player> _players;
        private List<Card> _deck;
        private BaseRuleSet _rules;
        private PotManager _potManager;
        private RoleManager _roleManager;
        private bool _isGameRunning = false;
        private bool _hasRaised = false;
        private bool _playerThinking = false;

        public TableServiceWorker(IHubContext<BasePokerHub> hubContext, BasePokerHub hub,
            BaseRuleSet ruleSet, IDeckFactory deckFactory)
        {
            _hubContext = hubContext;
            _rules = ruleSet;
            _deckFactory = deckFactory;

            if (_players == null)
            {
                _players = new List<Player>();
            }
            if (_potManager == null)
            {
                _potManager = new PotManager();
            }
            if (_roleManager == null)
            {
                _roleManager = new RoleManager();
            }


            // Subscribe to event
            hub.NewPlayerConnectedEvent += AddNewPlayerToGame;
            hub.PlayerHasDisconnectedEvent += RemovePlayerFromGame;
            hub.PlayerIsReadyEvent += PlayerIsReadyToPlay;
            hub.PlayerMadeActionEvent += PlayerMadeAction;
        }

        private async void PlayerMadeAction(string user, string action, int amount, string clientId)
        {
            Player player = _players.Where(p => p.ClientId == clientId).First();

            switch (action)
            {
                case "call":
                    _potManager.AddToPot(amount, player);
                    await _hubContext.Clients.All
                        .SendAsync("SendMessage", $"{player.Username} has called and added {amount} to the pot");
                    UpdatePot();
                    UpdatePlayerAmount(player);
                    break;
                case "raise":
                    _hasRaised = true;
                    _potManager.AddToPot(amount, player);
                    await _hubContext.Clients.All
                        .SendAsync("SendMessage", $"{player.Username} has raised the pot with {amount} turkey coins!");
                    UpdatePot();
                    UpdatePlayerAmount(player);
                    break;
                case "fold":
                    player.Active = false;
                    await _hubContext.Clients.All
                        .SendAsync("SendMessage", $"{player.Username} has folded");
                    break;
                case "check":
                    await _hubContext.Clients.All
                        .SendAsync("SendMessage", $"{player.Username} checks");
                    break;
            }
            _playerThinking = false;
        }

        private async void UpdatePot()
        {
            await _hubContext.Clients.All.SendAsync("UpdatePot", _potManager.TotalPotAmount);
        }

        private async void UpdatePlayerAmount(Player player)
        {
            await _hubContext.Clients.Client(player.ClientId).SendAsync("UpdateMoney", player.Money);
        }

        private async void AddNewPlayerToGame(string user, string clientId)
        {
            var newPlayer = new Player() { Username = user, ClientId = clientId, Active = false, Money = 200 };
            _players.Add(newPlayer);
            Console.WriteLine($"New player added, total number of users: {_players.Count()}");
            await _hubContext.Clients.All.SendAsync("SendMessage", newPlayer.Username);
        }

        private async void RemovePlayerFromGame(string user, string clientId)
        {
            var playerToRemove = _players.Find(player => player.ClientId == clientId);
            if (playerToRemove != null)
            {
                _players.Remove(playerToRemove);
                Console.WriteLine($"Player {user} removed from game, total number of players: {_players.Count()}");
                await _hubContext.Clients.All.SendAsync("SendMessage", $"{playerToRemove.Username} has left");
            }
        }
        private async void PlayerIsReadyToPlay(string user, string clientId)
        {
            var playerToBeReady = _players.Find(player => player.ClientId == clientId);
            if (playerToBeReady != null)
            {
                playerToBeReady.Active = true;
                Console.WriteLine($"Player {user} is ready to play, status is set to {playerToBeReady.Active}");
                await _hubContext.Clients.All.SendAsync("SendMessage", $"{playerToBeReady.Username} is ready!");
            }
            CheckIfGameCanBegin();
        }

        private void CheckIfGameCanBegin()
        {
            var numberOfPlayersReady = 0;
            foreach (var player in _players)
            {
                if (player.Active)
                {
                    numberOfPlayersReady++;
                }
            }
            if (numberOfPlayersReady >= _rules.MinimumPlayers
                && numberOfPlayersReady == _players.Count
                && _isGameRunning == false)
            {
                _isGameRunning = true;
                BeginGame();
                //Game is over and all players need to press ready again for a new round to begin
                _isGameRunning = false;
                foreach (Player player in _players)
                {
                    player.Active = false;
                }
            }
        }

        private async void BeginGame()
        {
            _deck = _deckFactory.GetNewDeck();
            _potManager.TotalPotAmount = 0;
            _roleManager.MoveRoles(_players);
            SetTurnOrder();
            //TODO Set players inactive if they have no cash and notify them
            PayBlindsAsync();


            _rules.DealCards(_players, _deck, 2);
            foreach (Player player in _players)
            {
                CardModel first = new CardModel(player.CardHand[0]);
                CardModel second = new CardModel(player.CardHand[1]);
                await _hubContext.Clients.Client(player.ClientId)
                    .SendAsync("GetPlayerCards", first, second);
            }

            for (int i = 0; i < 5; i++)
            {
                //Check if there are any players left to keep the rounds going
                if (_players.Count(p => p.Active) > 1)
                {
                    do
                    {
                        _hasRaised = false;
                        BettingRound();
                    } while (_hasRaised);
                    DealCommunityCards(i);
                }
                else
                    i = 5;
            }
            //Do showdown
            if (_players.Count(player => player.Active) > 1)
            {
                await _hubContext.Clients.All.SendAsync("Showdown", _players.Where(p => p.Active).ToList());
            }

            List<Player> winners = _rules.DetermineWinner(_players.Where(player => player.Active).ToList());
            await _hubContext.Clients.All.SendAsync("ShowWinners", winners);
            _potManager.PayOutPotToWinners(winners);
            winners.ForEach(player => UpdatePlayerAmount(player));
        }


        /// <summary>
        /// Force players with the <see cref="Role.BIG_BLIND"/> and <seealso cref="Role.SMALL_BLIND"/> to pay their blinds
        /// </summary>
        private async Task PayBlindsAsync()
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].Role == Role.BIG_BLIND)
                {
                    _potManager.AddToPot(_potManager.Big_Blind, _players[i]);
                    await _hubContext.Clients.All.SendAsync("SendMessage", $"{_players[i].Username} has paid {_potManager.Big_Blind} as big blind");
                }

                else if (_players[i].Role == Role.SMALL_BLIND)
                {
                    _potManager.AddToPot(_potManager.Small_Blind, _players[i]);
                    await _hubContext.Clients.All.SendAsync("SendMessage", $"{_players[i].Username} has paid {_potManager.Big_Blind} as small blind");
                }
            }
        }

        /// <summary>
        /// Deals a number of Cards depending on the round number, River(1), Turn(2), Flop(3)
        /// </summary>
        /// <param name="roundNumber"></param>
        private async void DealCommunityCards(int roundNumber)
        {
            if (roundNumber == 1)
            {
                Card tableFirstCard = _deck.FirstOrDefault();
                CardModel tableFirstCardModel = new CardModel(tableFirstCard);
                _deck.Remove(tableFirstCard);
                Card tableSecondCard = _deck.FirstOrDefault();
                CardModel tableSecondCardModel = new CardModel(tableSecondCard);
                _deck.Remove(tableSecondCard);
                Card tableThirdCard = _deck.FirstOrDefault();
                CardModel tableThirdCardModel = new CardModel(tableThirdCard);
                _deck.Remove(tableThirdCard);

                await _hubContext.Clients.All.SendAsync("GetFlop", tableFirstCardModel, tableSecondCardModel, tableThirdCardModel);
            }
            else if (roundNumber == 2)
            {
                await _hubContext.Clients.All.SendAsync("GetTurn", _deck.First());
                _deck.RemoveAt(0);
            }
            else if (roundNumber == 3)
            {
                await _hubContext.Clients.All.SendAsync("GetRiver", _deck.First());
                _deck.RemoveAt(0);
            }
        }

        private async void BettingRound()
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].Active)
                {
                    Player currentUser = _players[i];
                    _playerThinking = true;
                    await _hubContext.Clients.Client(currentUser.ClientId).SendAsync("ActionReady");
                    while (_playerThinking) ;
                }
            }
        }

        private void SetTurnOrder()
        {
            //Swap the order of the 2 players
            if (_players.Count == 2)
            {
                Player nextPlayer = _players[0];
                _players.Remove(nextPlayer);
                _players.Add(nextPlayer);
            }
            if (_players.Count >= 3)
            {
                int bigBlindIndex = -1;
                for (int i = 0; i < _players.Count; i++)
                {
                    if (_players[i].Role == Role.BIG_BLIND)
                    {
                        bigBlindIndex = i;
                        break;
                    }
                }

                if (bigBlindIndex >= 0)
                {
                    Player nextPlayer = _players[(bigBlindIndex + 1) % _players.Count];
                    _players.Remove(nextPlayer);
                    _players.Insert(0, nextPlayer);
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // TODO: some logic here!?
        }
    }
}
