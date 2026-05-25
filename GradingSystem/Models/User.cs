using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Models
{
    internal abstract class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public abstract string Role { get; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public abstract string GetRoleName();

        public abstract string GetUserName();

        public abstract string GetEmail();

        public abstract string GetRole();
    }
}
