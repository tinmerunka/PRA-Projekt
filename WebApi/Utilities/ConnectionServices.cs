using System.Text.Json;
using WebApi.DTOs;

namespace WebApi.Utilities
{
    /// <summary>
    /// Player score data for single-player quiz tracking
    /// </summary>
    public class PlayerScore
    {
        public int Id { get; set; }
        public QuestionDTO? Question { get; set; } = null;
        public bool Answered { get; set; } = false;
    }

    /// <summary>
    /// DTO for returning player scores 
    /// </summary>
    public class PlayerScoreDTO
    {
        public string Username { get; set; }
        public int Score { get; set; }
        public override string ToString()
        {
            return $"{Username}: {Score}";
        }
    }

    /// <summary>
    /// Service for handling quiz play functionality
    /// </summary>
    public interface IQuizPlayService
    {
        /// <summary>
        /// Gets the question at a specific position in a quiz
        /// </summary>
        /// <param name="quizId">The ID of the quiz</param>
        /// <param name="questionPosition">The position of the question</param>
        /// <returns>The question DTO or null if not found</returns>
        QuestionDTO? GetQuizQuestion(int quizId, int questionPosition);

        /// <summary>
        /// Records a player's answer and calculates score
        /// </summary>
        /// <param name="quizId">The ID of the quiz</param>
        /// <param name="questionId">The ID of the question</param>
        /// <param name="playerId">The ID of the player</param>
        /// <param name="answerIndex">The index of the selected answer</param>
        /// <param name="timeRemaining">Time remaining when answer was submitted</param>
        /// <returns>The score earned for this answer</returns>
        int RecordAnswer(int quizId, int questionId, int playerId, int answerIndex, int timeRemaining);

        /// <summary>
        /// Gets information about a quiz, including title and question count
        /// </summary>
        /// <param name="quizId">The ID of the quiz</param>
        /// <returns>Quiz details or null if not found</returns>
        object GetQuizInfo(int quizId);

        /// <summary>
        /// Gets final quiz results for a player
        /// </summary>
        /// <param name="playerId">The ID of the player</param>
        /// <param name="quizId">The ID of the quiz</param>
        /// <returns>The final score and other statistics</returns>
        object GetQuizResults(int playerId, int quizId);

        /// <summary>
        /// Creates a new quiz attempt record
        /// </summary>
        /// <param name="playerName">The name of the player</param>
        /// <param name="quizId">The ID of the quiz</param>
        /// <returns>The created record ID</returns>
        int StartQuiz(string playerName, int quizId);
    }

    /// <summary>
    /// Implementation of the quiz play service
    /// </summary>
    public class QuizPlayService : IQuizPlayService
    {
        private readonly Func<IDbService> _dbServiceFactory;

        public QuizPlayService(Func<IDbService> dbServiceFactory)
        {
            _dbServiceFactory = dbServiceFactory;
        }

        private IDbService GetDbService()
        {
            return _dbServiceFactory();
        }

        public QuestionDTO? GetQuizQuestion(int quizId, int questionPosition)
        {
            var dbService = GetDbService();
            return dbService.GetQuizQuestion(quizId, questionPosition);
        }

        public int RecordAnswer(int quizId, int questionId, int playerId, int answerIndex, int timeRemaining)
        {
            var dbService = GetDbService();

            // Get the question to calculate score
            var question = dbService.GetQuizQuestion(quizId, questionId);
            if (question == null)
                return 0;

            // Calculate score based on time remaining (similar to WebSocket version)
            int maxPoints = question.QuestionMaxPoints;
            int calculatedScore = (int)(maxPoints * (0.5 + (timeRemaining / (double)question.QuestionTime) * 0.5));

            // Update player score
            var quizRecord = dbService.GetQuizRecord(playerId);
            if (quizRecord == null)
                return 0;

            quizRecord.Score += calculatedScore;
            dbService.UpdateQuizRecord(quizRecord);

            return calculatedScore;
        }

        public object GetQuizInfo(int quizId)
        {
            var dbService = GetDbService();
            var quiz = dbService.GetQuizById(quizId);
            if (quiz == null)
                return null;

            return new
            {
                quizId = quiz.QuizId,
                title = quiz.Title,
                description = quiz.Description,
                totalQuestions = quiz.Questions.Count()
            };
        }

        public object GetQuizResults(int playerId, int quizId)
        {
            var dbService = GetDbService();
            var quizRecord = dbService.GetQuizRecord(playerId);
            if (quizRecord == null)
                return null;

            var quiz = dbService.GetQuizById(quizId);
            if (quiz == null)
                return null;

            return new
            {
                quizTitle = quiz.Title,
                playerName = quizRecord.PlayerName,
                score = quizRecord.Score,
                totalQuestions = quiz.Questions.Count()
            };
        }

        public int StartQuiz(string playerName, int quizId)
        {
            var dbService = GetDbService();

            // Generate a 6-character session ID instead of the longer format
            string sessionId = new Random().Next(100000, 999999).ToString(); // 6 digits

            // Create a new record for this attempt
            int recordId = dbService.AddNewPlayer(new QuizRecordDTO
            {
                PlayerName = playerName,
                QuizId = quizId,
                Score = 0,
                SessionId = sessionId
            });

            // Log the quiz start as history (optional)
            dbService.AddQuizHistory(new QuizHistoryDTO
            {
                PlayedAt = DateTime.Now,
                QuizId = quizId,
                WinnerName = playerName,
                WinnerId = 0 // Since this is solo play, no specific winner ID
            });

            return recordId;
        }
    }
}
