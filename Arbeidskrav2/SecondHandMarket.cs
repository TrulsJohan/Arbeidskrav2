namespace Arbeidskrav2
{
    public class SecondHandMarket
    {
        private readonly List<User> _users = new();
        public AuthService Auth { get; }
        
        public SecondHandMarket()
        {
            Auth = new AuthService(_users);
        }
    }
}