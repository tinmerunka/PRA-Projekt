using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class FullQuizDTO
    {
        public int QuizId { get; set; }

        [Required(ErrorMessage = "A title is required")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "A description is required")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "The ID of the author is required")]
        public int AuthorId { get; set; }

        public IEnumerable<QuestionDTO> Questions { get; set; } = new List<QuestionDTO>();
    }
}
