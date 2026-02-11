using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PC_Builder.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; } = string.Empty; // Новое поле
        public string Email { get; set; } = string.Empty; // Новое поле
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Admin" или "Client"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
