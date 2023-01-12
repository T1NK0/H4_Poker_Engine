using H4_Poker_Engine.Hubs;
using H4_Poker_Engine.Interfaces;
using H4_Poker_Engine.Models;
using H4_Poker_Engine.PokerLogic;
using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography.X509Certificates;

namespace H4_Poker_Engine.Services
{
    public class TableServiceWorker : BackgroundService
    {
        private readonly BasePokerHub _hub;
        private IDeckFactory _deckFactory;
        private List<Player> _players;
        private List<Card> _deck;
        private BaseRuleSet _rules;
        private PotManager _potManager;
        private RoleManager _roleManager;
        private PlayerActionEvaluator _playerActionManager;
        private bool _isGameRunning = false;
        private bool _hasRaised = false;
        private bool _playerThinking = false;

        public TableServiceWorker(BasePokerHub hub,
            BaseRuleSet ruleSet, IDeckFactory deckFactory)
        {
            //_hub = hubContext;
            _hub = hub;
            _rules = ruleSet;
            _deckFactory = deckFactory;

            if (_players == null)
            {
                _players = new List<Player>();
            }

            //DI all of these classes, if able, use interfaces
            if (_potManager == null)
            {
                _potManager = new PotManager();
            }
            if (_roleManager == null)
            {
                _roleManager = new RoleManager();
            }
            if (_playerActionManager == null)
            {
                _playerActionManager = new PlayerActionEvaluator();
            }


            // Subscribe to event
            _hub.NewPlayerConnectedEvent += AddNewPlayerToGameAsync;
            _hub.PlayerHasDisconnectedEvent += RemovePlayerFromGameAsync;
            _hub.PlayerIsReadyEvent += PlayerIsReadyToPlayAsync;
            _hub.PlayerMadeActionEvent += PlayerMadeActionAsync;
        }

        private async void PlayerMadeActionAsync(string user, string action, int raiseAmount, string clientId)
        {
            _playerThinking = false;

            Console.WriteLine("------- Enters: Player Made Action -------");
            //This method would be a good talking point, as it prob breaks the S in solid.
            Player player = _players.Where(p => p.ClientId == clientId).First();

            if (_currentPlayer.ClientId == player.ClientId)
            {
                switch (action)
                {
                    case "call":
                        int callAmount = _potManager.CallPot(player);
                        await _hub.Clients.All
                            .SendAsync("SendMessage", $"{player.Username} has called and added {callAmount} to the pot");
                        UpdatePotAsync();
                        UpdatePlayerAmountAsync(player);
                        break;
                    case "raise":
                        _hasRaised = true;
                        _potManager.RaisePot(raiseAmount, player);
                        await _hub.Clients.All
                            .SendAsync("SendMessage", $"{player.Username} has raised the pot with {raiseAmount} turkey coins!");
                        UpdatePotAsync();
                        UpdatePlayerAmountAsync(player);
                        break;
                    case "fold":
                        player.Active = false;
                        await _hub.Clients.All
                            .SendAsync("SendMessage", $"{player.Username} has folded");
                        break;
                    case "check":
                        await _hub.Clients.All
                            .SendAsync("SendMessage", $"{player.Username} checks");
                        break;
                }

                SetCurrentPlayer(player);
            }

            BettingRoundAsync(_currentPlayer);
        }

        private async void UpdatePotAsync()
        {
            Console.WriteLine("------- Enters: Update Pot -------");
            await _hub.Clients.All.SendAsync("UpdatePot", _potManager.TotalPotAmount);
        }

        private async void UpdatePlayerAmountAsync(Player player)
        {
            Console.WriteLine("------- Enters: Update Player Amount -------");
            await _hub.Clients.Client(player.ClientId).SendAsync("UpdateMoney", player.Money);
        }

        private async void AddNewPlayerToGameAsync(string user, string clientId)
        {
            Console.WriteLine("------- Enters: Add New Player To Game -------");
            var newPlayer = new Player(user, clientId);
            _players.Add(newPlayer);
            Console.WriteLine($"New player added, total number of users: {_players.Count()}");
            await _hub.Clients.All.SendAsync("SendMessage", newPlayer.Username);
        }

        private async void RemovePlayerFromGameAsync(string user, string clientId)
        {
            Console.WriteLine("------- Enters: Remove player from game -------");
            var playerToRemove = _players.Find(player => player.ClientId == clientId);
            if (playerToRemove != null)
            {
                _players.Remove(playerToRemove);
                Console.WriteLine($"Player {user} removed from game, total number of players: {_players.Count()}");
                await _hub.Clients.All.SendAsync("SendMessage", $"{playerToRemove.Username} has left");
            }
        }
        private async void PlayerIsReadyToPlayAsync(string user, string clientId)
        {
            Console.WriteLine("------- Enters: Player is ready to play -------");
            var playerToBeReady = _players.Find(player => player.ClientId == clientId);
            if (playerToBeReady != null)
            {
                playerToBeReady.Active = true;
                Console.WriteLine($"Player {user} is ready to play, status is set to {playerToBeReady.Active}");
                await _hub.Clients.All.SendAsync("SendMessage", $"{playerToBeReady.Username} is ready!");
            }
            CheckIfGameCanBegin();
        }

        private void CheckIfGameCanBegin()
        {
            Console.WriteLine("------- Enters: Check If Game Can Begin -------");
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
                BeginGameAsync();
                //Game is over and all players need to press ready again for a new round to begin
                _isGameRunning = false;
                //foreach (Player player in _players)
                //{
                //    player.Active = false;
                //}
            }
        }

        private async void BeginGameAsync()
        {
            Console.WriteLine("------- Enters: Begin Game -------");
            _deck = _deckFactory.GetNewDeck();
            _potManager.TotalPotAmount = 0;

            //if there's no big blind, we assume there is no small blind either.
            if (!_players.Any(p => p.Role == Role.BIG_BLIND))
                SetBlinds();


            if (!_roleManager.MoveRoles(_players, _potManager.Small_Blind, _potManager.Big_Blind))
            {
                await _hub.Clients.All
                    .SendAsync("SendMessage", "Something went wrong with Moving roles, there was no suitable blind holder(s)");
                return;
            }
            SetTurnOrder();

            //TODO Set players inactive(done) if they have no cash and notify them
            //TODO Set set- and payblinds into ruleset probably
            PayBlindsAsync();


            _rules.DealCards(_players, _deck, 2);
            foreach (Player player in _players)
            {
                await _hub.Clients.Client(player.ClientId)
                    .SendAsync("GetPlayerCards", player.CardHand[0], player.CardHand[1]);
            }

            //for (int i = 0; i < 5; i++)
            //{
            //Check if there are any players left to keep the rounds going
            //if (_players.Count(p => p.Active) > 1)
            //{
            //do
            //{
            _hasRaised = false;
            _currentPlayer = _players[0];
            BettingRoundAsync(_players[0]);
            //} while (_hasRaised);
            //DealCommunityCardsAsync(i);
            //for (int j = 0; j < _players.Count; j++)
            //{
            //    _players[i].CurrentBetInRound = 0;
            //}
            //}
            //    else
            //        i = 5;
            //}
            //Do showdown           
        }

        private void SetBlinds()
        {
            Console.WriteLine("------- Enters: Set Blinds -------");
            //Set blinds randomly at start of game
            int smallBlindIndex = new Random().Next(0, _players.Count);
            _players[smallBlindIndex].Role = Role.SMALL_BLIND;
            if (smallBlindIndex == _players.Count - 1)
                _players[0].Role = Role.BIG_BLIND;
            else
                _players[smallBlindIndex + 1].Role = Role.BIG_BLIND;
        }


        /// <summary>
        /// Force players with the <see cref="Role.BIG_BLIND"/> and <seealso cref="Role.SMALL_BLIND"/> to pay their blinds
        /// </summary>
        private async void PayBlindsAsync()
        {
            Console.WriteLine("------- Enters: Pay Blinds -------");
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].Role == Role.SMALL_BLIND)
                {
                    _potManager.CurrentCallAmount = _potManager.Small_Blind;
                    _potManager.CallPot(_players[i]);
                    await _hub.Clients.All.SendAsync("SendMessage", $"{_players[i].Username} has paid {_potManager.Small_Blind} as small blind");
                }
                else if (_players[i].Role == Role.BIG_BLIND)
                {
                    _potManager.CurrentCallAmount = _potManager.Big_Blind;
                    _potManager.CallPot(_players[i]);
                    await _hub.Clients.All.SendAsync("SendMessage", $"{_players[i].Username} has paid {_potManager.Big_Blind} as big blind");
                }
            }
        }

        /// <summary>
        /// Deals a number of Cards depending on the round number, River(1), Turn(2), Flop(3)
        /// </summary>
        /// <param name="roundNumber"></param>
        private async void DealCommunityCardsAsync(int roundNumber)
        {
            Console.WriteLine("------- Enters: Deal Community Cards -------");
            if (roundNumber == 1)
            {
                Card tableFirstCard = _deck.FirstOrDefault();
                _deck.Remove(tableFirstCard);
                Card tableSecondCard = _deck.FirstOrDefault();
                _deck.Remove(tableSecondCard);
                Card tableThirdCard = _deck.FirstOrDefault();
                _deck.Remove(tableThirdCard);
                await _hub.Clients.All
                    .SendAsync("GetFlop",
                    tableFirstCard,
                    tableSecondCard,
                    tableThirdCard);
            }
            else if (roundNumber == 2)
            {
                await _hub.Clients.All.SendAsync("GetTurn", _deck.First());
                _deck.RemoveAt(0);
            }
            else if (roundNumber == 3)
            {
                await _hub.Clients.All.SendAsync("GetRiver", _deck.First());
                _deck.RemoveAt(0);
            }

        }

        private async void BettingRoundAsync(Player currentPlayer)
        {
            Console.WriteLine("------- Enters: Betting Round -------");
            //for (int i = 0; i < _players.Count; i++)
            //{
            //if (_players[i].Active)
            //{
            //Player currentUser = _players[i];
            //_playerThinking = true;
            Console.WriteLine($"******* Userturn: {currentPlayer.Username} *******");
            await _hub.Clients.Client(currentPlayer.ClientId)
                .SendAsync("ActionReady", _playerActionManager.GetValidActions(currentPlayer, _potManager, _hasRaised));
            //while (_playerThinking) ;
            //}
            //}
        }

        Player _currentPlayer;
        int roundCounter = 0;
        private async void SetCurrentPlayer(Player previousPlayer)
        {
            int indexOfPrevious = _players.IndexOf(previousPlayer);
            if (indexOfPrevious + 1 < _players.Count)
            {
                for (int i = indexOfPrevious + 1; i < _players.Count; i++)
                {
                    if (_players[i].Active)
                    {
                        _currentPlayer = _players[i];
                        i = _players.Count;
                    }
                }
            }
            else if (indexOfPrevious + 1 == _players.Count && _hasRaised)
            {
                _hasRaised = false;
                _currentPlayer = _players.Where(p => p.Active).First();
            }
            else if (indexOfPrevious + 1 == _players.Count && !_hasRaised)
            {
                roundCounter++;
                DealCommunityCardsAsync(roundCounter);
                _currentPlayer = _players.Where(p => p.Active).First();

                if (_players.Count(player => player.Active) > 1 && roundCounter == 4)
                {
                    await _hub.Clients.All.SendAsync("Showdown", _players.Where(p => p.Active).ToList());
                }

                if (roundCounter == 5 || _players.Count(p => p.Active) == 1)
                {
                    List<Player> winners = _rules.DetermineWinner(_players.Where(player => player.Active).ToList());
                    await _hub.Clients.All.SendAsync("ShowWinners", winners);
                    _potManager.PayOutPotToWinners(winners);
                    winners.ForEach(player => UpdatePlayerAmountAsync(player));
                }
            }
        }

        private void SetTurnOrder()
        {
            Console.WriteLine("------- Enters: Set Turn Order -------");
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
            Console.WriteLine("------- Enters: Execute -------");
            // TODO: some logic here!?
        }
    }
}
