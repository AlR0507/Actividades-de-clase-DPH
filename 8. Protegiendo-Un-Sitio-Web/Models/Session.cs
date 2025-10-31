using Protegiendo_un_sitio_web.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Protegiendo_un_sitio_web.Models
{
    public class UserSession
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string SessionId { get; set; } = string.Empty; // base64url 128-bit token
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastActivityUtc { get; set; }
        public bool IsRevoked { get; set; }
        public int IdleTimeoutMinutes { get; set; } = 5; // inactivity window
    }
}