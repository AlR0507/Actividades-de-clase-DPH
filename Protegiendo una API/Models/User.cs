using System;
using System.Collections.Generic;

namespace Comprehension.Models
{
    public enum AppRole { User = 0, Admin = 1 } // (Puntos extra)

    public class User
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }

        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }

        public AppRole Role { get; set; } = AppRole.User;

        public ICollection<SessionToken> Tokens { get; set; } = new List<SessionToken>();
    }
}
