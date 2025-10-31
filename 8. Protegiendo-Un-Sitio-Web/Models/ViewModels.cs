using System.ComponentModel.DataAnnotations;


namespace Protegiendo_un_sitio_web.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, StringLength(32, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;


        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;


        [Required, DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }


        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;


        [StringLength(100)]
        public string? FullName { get; set; }
    }


    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;


        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}