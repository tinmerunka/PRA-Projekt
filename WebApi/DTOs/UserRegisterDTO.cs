using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class UserRegisterDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "A password should be at least 8 characters long")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "A first name is required")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "An email is required")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "A last name is required")]
        public string LastName { get; set; } = null!;
    }
}
