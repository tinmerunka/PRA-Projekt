using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class AnswerDTO
    {
        public int AnswerId { get; set; }

        [Required(ErrorMessage = "The answer text is required")]
        public string AnswerText { get; set; } = null!;

        [Required(ErrorMessage = "A certified hood classic is required")]
        public bool Correct { get; set; }
    }
}
