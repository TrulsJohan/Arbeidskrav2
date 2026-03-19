using System;
using System.Collections.Generic;

namespace Arbeidskrav2
{
    /// <summary>
    /// Handles all authentication logic (register, login, logout).
    /// Uses a shared user list with the market (in-memory storage).
    /// </summary>
    public class AuthService
    {
        private readonly List<User> _allUsers;
        private User? _currentUser;
        
        public AuthService(List<User> userRepository)
        {
            _allUsers = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }
        
        /// <summary>
        /// Currently logged-in user (null if guest).
        /// </summary>
        public User? CurrentUser => _currentUser;

        /// <summary>
        /// True if a user is logged in.
        /// </summary>
        public bool IsLoggedIn => _currentUser != null;

        /// <summary>
        /// Username of the current user (null if not logged in).
        /// </summary>
        public string? CurrentUsername => _currentUser?.Username;

        /// <summary>
        /// Attempts to log in. Returns true on success.
        /// Fixed: correct password check logic.
        /// </summary>
        public bool Login(string username, string password)
        {
            var user = _allUsers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user == null || !user.VerifyPassword(password))
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

        /// <summary>
        /// Registers a new user and automatically logs them in.
        /// Throws if username taken.
        /// </summary>
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