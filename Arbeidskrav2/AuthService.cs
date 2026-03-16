namespace Arbeidskrav2
{
    public class AuthService
    {
        public readonly List<User> _allUsers;
        private User?  _currentUser;
        
        public AuthService(List<User> userRepository)
        {
            _allUsers = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }
        
        public User? CurrentUser => _currentUser;
        public bool IsLoggedIn() => _currentUser != null;
        public string CurrentUsername() => _currentUser?.Username;

        public bool Login(string username, string password)
        {
            var user = _allUsers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user == null || user.VerifyPassword(password))
            {
                _currentUser = null;
                return false;
            }
            
            _currentUser = user;
            Console.WriteLine($"Welcome back, {user.Username}!");
            return true;
        }

        public void Logout()
        {
            if (_currentUser != null)
            {
                Console.WriteLine($"Goodbye, {_currentUser.Username}!");
                _currentUser = null;
            }
        }

        public User Register(string username, string password)
        {
            if (_allUsers.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Username '{username}' is already taken.");
            
            var newUser = new User(username, password);
            _allUsers.Add(newUser);
            _currentUser = newUser;
            
            Console.WriteLine($"Account created! You are now logged in as {username}.");
            return newUser;
        }
    }
}