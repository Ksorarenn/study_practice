using PC_Builder.Core.Models;
using PC_Builder.Data;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace PC_Builder.WinForms.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return null;
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }

        public bool Register(string fullName, string username, string email, string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Заполните все обязательные поля";
                return false;
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                errorMessage = "Пользователь с таким логином уже существует";
                return false;
            }
            
            if (_context.Users.Any(u => u.Email == email))
            {
                 errorMessage = "Пользователь с таким Email уже существует";
                 return false;
            }

            try
            {
                var newUser = new User
                {
                    FullName = fullName,
                    Username = username,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = "Client",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Ошибка при регистрации: {ex.Message}";
                return false;
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            try 
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            catch
            {
                // Если хеш в старом формате или некорректен
                return false;
            }
        }
    }
}
