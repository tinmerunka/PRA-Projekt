using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class UserChangePasswordDTO
    {

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "A password should be at least 8 characters long")]
        public string OldPassword { get; set; } = null!;
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "A password should be at least 8 characters long")]
        public string NewPassword { get; set; } = null!;
    }
}
