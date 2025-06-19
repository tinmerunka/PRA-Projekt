using System.ComponentModel.DataAnnotations;
using WebApi.Models;

namespace WebApi.DTOs
{
    public class QuestionDTO
    {
        public int QuestionId { get; set; }
        [Required(ErrorMessage = "The question text is required")]
        public string QuestionText { get; set; } = string.Empty;
        [Required(ErrorMessage = "A question needs answers")]
        public IEnumerable<AnswerDTO> Answers { get; set; } = Enumerable.Empty<AnswerDTO>();
        [Required(ErrorMessage = "A question needs a position")]
        public int QuestionPosition { get; set; }
        [Required(ErrorMessage = "A question needs a specified time")]
        public int QuestionTime { get; set; }
        [Required(ErrorMessage = "A question needs a maximum amount of points")]
        public int QuestionMaxPoints { get; set; }
        [Required(ErrorMessage = "A question needs to belong to a quiz")]
        public int QuizId { get; set; }
    }

}