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
                    _potManager.AddToPot(amount);
                    await _hubContext.Clients.All.SendAsync("PlayerCall", player.Username, amount);
                    UpdatePot();
                    break;
                case "raise":
                    _potManager.AddToPot(amount);
                    _hasRaised = true;
                    await _hubContext.Clients.All.SendAsync("PlayerRaise", player.Username, amount);
                    UpdatePot();
                    break;
                case "fold":
                    player.Active = false;
                    List<Player> activePlayers = _players.Where(p => p.Active).ToList();
                    for (int i = 0; i < activePlayers.Count; i++)
                    {
                        await _hubContext.Clients.Client(activePlayers[i].ClientId)
                            .SendAsync("PlayerFold", activePlayers[i].Username);
                    }
                    break;
                case "check":
                    await _hubContext.Clients.All.SendAsync("PlayerCheck", player.Username);
                    break;
            }
            _playerThinking = false;
        }

        private async void UpdatePot()
        {
            await _hubContext.Clients.All.SendAsync("UpdatePot", _potManager.TotalPotAmount);
        }

        private async void AddNewPlayerToGame(string user, string message, string clientId)
        {
            var newPlayer = new Player() { Username = user, ClientId = clientId, Active = false };
            _players.Add(newPlayer);
            Console.WriteLine($"New player added, total number of users: {_players.Count()}");
            await _hubContext.Clients.All.SendAsync("PlayerJoin", newPlayer.Username);
        }

        private async void RemovePlayerFromGame(string user, string message, string clientId)
        {
            var playerToRemove = _players.Find(player => player.ClientId == clientId);
            if (playerToRemove != null)
            {
                _players.Remove(playerToRemove);
                Console.WriteLine($"Player {user} removed from game, total number of players: {_players.Count()}");
                await _hubContext.Clients.All.SendAsync("PlayerLeave", playerToRemove.Username);
            }
        }
        private async void PlayerIsReadyToPlay(string user, string message, string clientId)
        {
            var playerToBeReady = _players.Find(player => player.ClientId == clientId);
            if (playerToBeReady != null)
            {
                playerToBeReady.Active = true;
                Console.WriteLine($"Player {user} is ready to play, status is set to {playerToBeReady.Active}");
                await _hubContext.Clients.All.SendAsync("PlayerReady", playerToBeReady.Username);
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

            _roleManager.MoveRoles(_players);
            SetTurnOrder();
            PayBlinds();


            _rules.DealCards(_players, _deck, 2);
            foreach (Player player in _players)
            {
                await _hubContext.Clients.Client(player.ClientId)
                    .SendAsync("GetPlayerCards", player.CardHand[0], player.CardHand[1]);
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
        }

        private void PayBlinds()
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].Role == Role.BIG_BLIND)
                {
                    _potManager.AddToPot(_potManager.Big_Blind);
                    _players[i].Money -= _potManager.Big_Blind;
                }
                else if (_players[i].Role == Role.SMALL_BLIND)
                {
                    _potManager.AddToPot(_potManager.Small_Blind);
                    _players[i].Money -= _potManager.Small_Blind;
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
                _deck.Remove(tableFirstCard);
                Card tableSecondCard = _deck.FirstOrDefault();
                _deck.Remove(tableSecondCard);
                Card tableThirdCard = _deck.FirstOrDefault();
                _deck.Remove(tableThirdCard);
                await _hubContext.Clients.All.SendAsync("GetFlop", tableFirstCard, tableSecondCard, tableThirdCard);
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
