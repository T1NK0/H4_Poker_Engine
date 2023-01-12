namespace H4_Poker_Engine.Models
{
    public enum Role
    {
        NONE,
        BIG_BLIND,
        SMALL_BLIND,
        //DEALER
    };

    //TODO give player turkey coins
    public class Player
    {
        private string _username;
        private string _clientId;
        private bool _active;
        private int _money;
        private int _currentBetInRound;
        private Role role = Role.NONE;        

        private List<Card> _cardHand;

        public Player(string username, string clientId)
        {
            _username = username;
            _clientId = clientId;
            _active = false;
            _money = 200;
            _currentBetInRound = 0;
            _cardHand = new List<Card>();
        }

        //For tests
        public Player()
        {

        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }
        public bool Active
        {
            get { return _active; }
            set { _active = value; }
        }
        public int Money
        {
            get { return _money; }
            set { _money = value; }
        }
        public int CurrentBetInRound
        {
            get { return _currentBetInRound; }
            set { _currentBetInRound = value; }
        }
        public List<Card> CardHand
        { 
            get { return _cardHand; } 
            set { _cardHand = value; } 
        }
        public Role Role
        {
            get { return role; }
            set { role = value; }
        }

    }
}
