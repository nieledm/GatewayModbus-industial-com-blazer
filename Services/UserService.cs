// Services/UserService.cs
using System.Text.Json;
using System.IO;

namespace DL6000WebConfig.Services
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; } // Em produção, armazene hash, não a senha em texto puro
    }

    public class UserService
    {
        private const string UsersFile = "users.json";
        private List<User> _users;

        public UserService()
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            if (File.Exists(UsersFile))
            {
                var json = File.ReadAllText(UsersFile);
                _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            else
            {
                _users = new List<User>();
                // Usuário admin padrão
                _users.Add(new User { Username = "admin", Password = "admin123" });
                SaveUsers();
            }
        }

        private void SaveUsers()
        {
            var json = JsonSerializer.Serialize(_users);
            File.WriteAllText(UsersFile, json);
        }

        public bool ValidateUser(string username, string password)
        {
            return _users.Any(u => u.Username == username && u.Password == password);
        }

        public void AddUser(string username, string password)
        {
            if (!_users.Any(u => u.Username == username))
            {
                _users.Add(new User { Username = username, Password = password });
                SaveUsers();
            }
        }
    }
}