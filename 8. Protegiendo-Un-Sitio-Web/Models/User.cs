using System.ComponentModel.DataAnnotations;


namespace Protegiendo_un_sitio_web.Models
{
    public class User
    {
        public int Id { get; set; }


        [Required, MaxLength(32)]
        public string Username { get; set; } = string.Empty;


        [Required, MaxLength(200)]
        public string Email { get; set; } = string.Empty;


        [Required]
        public DateTime DateOfBirth { get; set; }


        // Stored password in the format: {iterations}.{saltBase64}.{hashBase64}
        [Required]
        public string PasswordHash { get; set; } = string.Empty;


        // Optional personal display name
        [MaxLength(100)]
        public string? FullName { get; set; }
    }
}