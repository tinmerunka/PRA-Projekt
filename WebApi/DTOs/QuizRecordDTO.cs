using WebApi.Models;

namespace WebApi.DTOs
{
    public class QuizRecordDTO
    {
        public int Id { get; set; }

        public int QuizId { get; set; }

        public string SessionId { get; set; } = null!;

        public string PlayerName { get; set; } = null!;

        public int? Score { get; set; }
    }
}
