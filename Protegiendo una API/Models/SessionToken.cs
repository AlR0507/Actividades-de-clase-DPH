using System;

namespace Comprehension.Models
{
    public class SessionToken
    {
        public Guid Id { get; set; }
        public required byte[] TokenHash { get; set; }  // SHA-256 del token emitido
        public DateTime IssuedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public bool Revoked { get; set; }

        public Guid UserId { get; set; }
        public required User User { get; set; }
    }
}
