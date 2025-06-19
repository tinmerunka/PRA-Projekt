using WebApi.Models;

namespace WebApi.DTOs
{
    public class QuizHistoryDTO
    {
        public int Id { get; set; }

        public DateTime PlayedAt { get; set; }

        public int WinnerId { get; set; }

        public string WinnerName { get; set; } = null!;

        public int QuizId { get; set; }
    }
}
