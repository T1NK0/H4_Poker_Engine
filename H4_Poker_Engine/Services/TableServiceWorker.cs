using H4_Poker_Engine.Hubs;
using H4_Poker_Engine.Interfaces;
using H4_Poker_Engine.Models;
using H4_Poker_Engine.PokerLogic;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        private PlayerActionEvaluator _playerActionManager;
        private bool _isGameRunning = false;
        private bool _hasRaised = false;
        private Player _currentPlayer;
        private int roundCounter = 0;
        private int _endingPlayerIndex = 0;

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
            hub.NewPlayerConnectedEvent += AddNewPlayerToGameAsync;
            hub.PlayerHasDisconnectedEvent += RemovePlayerFromGameAsync;
            hub.PlayerIsReadyEvent += PlayerIsReadyToPlayAsync;
            hub.PlayerMadeActionEvent += PlayerMadeActionAsync;
        }

        private async void PlayerMadeActionAsync(string user, string action, int raiseAmount, string clientId)
        {
            Console.WriteLine("------- Enters: Player Made Action -------");
            //This method would be a good talking point, as it prob breaks the S in solid.
            Player playerJustPlayed = _players.Where(p => p.ClientId == clientId).First();

            SetNextPlayer(playerJustPlayed);

            if (_currentPlayer.ClientId == playerJustPlayed.ClientId)
            {
                switch (action)
                {
                    case "call":
                        int callAmount = _potManager.CallPot(playerJustPlayed);
                        await _hub.Clients.All
                            .SendAsync("SendMessage", $"{playerJustPlayed.Username} has called and added {callAmount} to the pot");
                        UpdatePotAsync();
                        UpdatePlayerAmountAsync(playerJustPlayed);
                        break;
                    case "raise":
                        int index = _players.IndexOf(playerJustPlayed);
                        if (index > 0)
                        {
                            //Used if there are people before the raising player, who need to be able to raise again
                            _endingPlayerIndex = index;
                            _hasRaised = true;
                        }
                        _potManager.RaisePot(raiseAmount, playerJustPlayed);
                        await _hub.Clients.All
                            .SendAsync("SendMessage", $"{playerJustPlayed.Username} has raised the pot with {raiseAmount} turkey coins!");
                        UpdatePotAsync();
                        UpdatePlayerAmountAsync(playerJustPlayed);
                        break;
                    case "fold":
                        playerJustPlayed.Active = false;
                        await _hub.Clients.All
                            .SendAsync("SendMessage", $"{playerJustPlayed.Username} has folded");
                        break;
                    case "check":
                        await _hub.Clients.All
                            .SendAsync("SendMessage", $"{playerJustPlayed.Username} checks");
                        break;
                }
            }

        private async void UpdatePotAsync()
        {
            await _hubContext.Clients.All.SendAsync("UpdatePot", _potManager.TotalPotAmount);
        }

        private async void UpdatePlayerAmountAsync(Player player)
        {
            await _hubContext.Clients.Client(player.ClientId).SendAsync("UpdateMoney", player.Money);
        }

        private async void AddNewPlayerToGameAsync(string user, string message, string clientId)
        {
            var newPlayer = new Player(user, clientId);
            _players.Add(newPlayer);
            Console.WriteLine($"New player added, total number of users: {_players.Count()}");
            await _hubContext.Clients.All.SendAsync("SendMessage", newPlayer.Username);
        }

        private async void RemovePlayerFromGameAsync(string user, string message, string clientId)
        {
            var playerToRemove = _players.Find(player => player.ClientId == clientId);
            if (playerToRemove != null)
            {
                _players.Remove(playerToRemove);
                Console.WriteLine($"Player {user} removed from game, total number of players: {_players.Count()}");
                await _hubContext.Clients.All.SendAsync("SendMessage", $"{playerToRemove.Username} has left");
            }
        }
        private async void PlayerIsReadyToPlayAsync(string user, string message, string clientId)
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
                BeginGameAsync();
                //Game is over and all players need to press ready again for a new round to begin
                _isGameRunning = false;
                foreach (Player player in _players)
                {
                    player.Active = false;
                }
            }
        }

        private async void BeginGameAsync()
        {
            _deck = _deckFactory.GetNewDeck();
            _potManager.TotalPotAmount = 0;
            _endingPlayerIndex = _players.Count - 1;
            //if there's no big blind, we assume there is no small blind either.
            if (!_players.Any(p => p.Role == Role.BIG_BLIND))
                SetBlinds();


            if (!_roleManager.MoveRoles(_players, _potManager.Small_Blind, _potManager.Big_Blind))
            {
                await _hubContext.Clients.All
                    .SendAsync("SendMessage", "Something went wrong with Moving roles, there was no suitable blind holder(s)");
                return;
            } 
            SetTurnOrder();

            //TODO Set players inactive(done) if they have no cash and notify them
            //TODO Set set- and payblinds into ruleset probably
            await PayBlindsAsync();


            _rules.DealCards(_players, _deck, 2);
            foreach (Player player in _players)
            {
                await _hubContext.Clients.Client(player.ClientId)
                    .SendAsync("GetPlayerCards", JsonSerializer.Serialize(player.CardHand[0]), JsonSerializer.Serialize(player.CardHand[1]));
            }

            for (int i = 0; i < 5; i++)
            {
                //Check if there are any players left to keep the rounds going
                if (_players.Count(p => p.Active) > 1)
                {
                    do
                    {
                        _hasRaised = false;
                        BettingRoundAsync();
                    } while (_hasRaised);
                    DealCommunityCardsAsync(i);
                    for (int j = 0; j < _players.Count; j++)
                    {
                        _players[i].CurrentBetInRound = 0;
                    }
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
            winners.ForEach(player => UpdatePlayerAmountAsync(player));
        }

        private void SetBlinds()
        {
            //Set blinds randomly at start of game
            int smallBlindIndex = new Random().Next(0, _players.Count);
            _players[smallBlindIndex].Role = Role.SMALL_BLIND;
            if (smallBlindIndex == _players.Count)
                _players[0].Role = Role.BIG_BLIND;
            else
                _players[smallBlindIndex + 1].Role = Role.BIG_BLIND;
        }


        /// <summary>
        /// Force players with the <see cref="Role.BIG_BLIND"/> and <seealso cref="Role.SMALL_BLIND"/> to pay their blinds
        /// </summary>
        private async Task PayBlindsAsync()
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].Role == Role.SMALL_BLIND)
                {
                    _potManager.CurrentCallAmount = _potManager.Small_Blind;
                    _potManager.CallPot(_players[i]);
                    await _hubContext.Clients.All.SendAsync("SendMessage", $"{_players[i].Username} has paid {_potManager.Small_Blind} as small blind");
                }
                else if (_players[i].Role == Role.BIG_BLIND)
                {
                    _potManager.CurrentCallAmount = _potManager.Big_Blind;
                    _potManager.CallPot(_players[i]);
                    await _hubContext.Clients.All.SendAsync("SendMessage", $"{_players[i].Username} has paid {_potManager.Big_Blind} as big blind");
                }
            }
        }

        /// <summary>
        /// Deals a number of Cards depending on the round number, River(1), Turn(2), Flop(3)
        /// </summary>
        /// <param name="roundNumber"></param>
        private async void DealCommunityCardsAsync(int roundNumber)
        {
            if (roundNumber == 1)
            {
                Card tableFirstCard = _deck.FirstOrDefault();
                _deck.Remove(tableFirstCard);
                Card tableSecondCard = _deck.FirstOrDefault();
                _deck.Remove(tableSecondCard);
                Card tableThirdCard = _deck.FirstOrDefault();
                _deck.Remove(tableThirdCard);
                await _hubContext.Clients.All
                    .SendAsync("GetFlop", 
                    JsonSerializer.Serialize(tableFirstCard), 
                    JsonSerializer.Serialize(tableSecondCard), 
                    JsonSerializer.Serialize(tableThirdCard));
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


        private async void SetNextPlayer(Player previousPlayer)
        {
            int indexOfPrevious = _players.IndexOf(previousPlayer);
            if (!_hasRaised)
            {
                //normal loop with no raises
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
                else if (indexOfPrevious + 1 == _players.Count)
                {
                    EndRound();
                }
            }
            else
            {
                if(indexOfPrevious + 1 == _endingPlayerIndex)
                {
                    _hasRaised = false;
                    EndRound();
                }
                else if (indexOfPrevious + 1 == _players.Count)
                {
                    _currentPlayer = _players.Where(p => p.Active).First();
                }
                else if (indexOfPrevious > _endingPlayerIndex)
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
                else
                {
                    for (int i = indexOfPrevious + 1; i < _endingPlayerIndex; i++)
                    {
                        if (_players[i].Active)
                        {
                            _currentPlayer = _players[i];
                            i = _players.Count;
                        }
                    }
                }
            }
        }

        private async void EndRound()
        {
            roundCounter++;
            _endingPlayerIndex = _players.Count - 1;
            DealCommunityCardsAsync(roundCounter);
            _currentPlayer = _players.Where(p => p.Active).First();
            _potManager.CurrentCallAmount = 0;
            foreach (Player player in _players)
            {
                if (player.Active)
                {
                    player.CurrentBetInRound = 0;
                }
            }

            if (_players.Count(player => player.Active) > 1 && roundCounter == 4)
            {
                await _hub.Clients.All.SendAsync("Showdown", _players.Where(p => p.Active).ToList());
            }

            if (roundCounter == 5 || _players.Count(p => p.Active) == 1)
            {
                for (int i = 0; i < _players.Count; i++)
                {
                    if (_players[i].Active)
                    {
                        _players[i].CardHand.AddRange(_communityCards);
                    }
                }
                List<Player> winners = _rules.DetermineWinner(_players.Where(player => player.Active).ToList());
                await _hub.Clients.All.SendAsync("ShowWinners", winners);
                _potManager.PayOutPotToWinners(winners);
                winners.ForEach(player => UpdatePlayerAmountAsync(player));
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
