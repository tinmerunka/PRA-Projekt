using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Utilities;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IDbService _dbService;
        private readonly IQuizPlayService _quizPlayService;

        public QuizController(IDbService dbService, IQuizPlayService quizPlayService)
        {
            _dbService = dbService;
            _quizPlayService = quizPlayService;
        }

        // GET: api/Quiz
        [HttpGet]
        public ActionResult<IEnumerable<QuizDTO>> GetAllQuizzes()
        {
            return Ok(_dbService.GetAllQuizzes());
        }

        // GET: api/Quiz/5
        [HttpGet("{id}")]
        public ActionResult<FullQuizDTO> GetQuiz(int id)
        {
            var quiz = _dbService.GetQuizById(id);
            if (quiz == null)
            {
                return NotFound();
            }
            return Ok(quiz);
        }

        // GET: api/Quiz/5/info
        [HttpGet("{id}/info")]
        public ActionResult<object> GetQuizInfo(int id)
        {
            var quiz = _dbService.GetQuizById(id);
            if (quiz == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                quizId = quiz.QuizId,
                title = quiz.Title,
                description = quiz.Description,
                totalQuestions = quiz.Questions.Count()
            });
        }

        // GET: api/Quiz/5/questions/1
        [HttpGet("{id}/questions/{position}")]
        public ActionResult<QuestionDTO> GetQuizQuestion(int id, int position)
        {
            var question = _dbService.GetQuizQuestion(id, position);
            if (question == null)
            {
                return NotFound();
            }
            return Ok(question);
        }

        // POST: api/Quiz/start
        [HttpPost("start")]
        public ActionResult<object> StartQuiz([FromBody] StartQuizRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.PlayerName) || request.QuizId <= 0)
            {
                return BadRequest("Invalid player name or quiz ID.");
            }

            // Check if quiz exists
            var quiz = _dbService.GetQuizById(request.QuizId);
            if (quiz == null)
            {
                return NotFound("Quiz not found.");
            }

            // Create a session ID
            string sessionId = $"solo-{Guid.NewGuid().ToString().Substring(0, 8)}";

            // Add player record
            int recordId = _dbService.AddNewPlayer(new QuizRecordDTO
            {
                PlayerName = request.PlayerName,
                QuizId = request.QuizId,
                SessionId = sessionId,
                Score = 0
            });

            // Return player ID and session
            return Ok(new
            {
                playerId = recordId,
                sessionId = sessionId
            });
        }

        // POST: api/Quiz/{quizId}/submit-answer
        [HttpPost("{quizId}/submit-answer")]
        public ActionResult<object> SubmitAnswer(int quizId, [FromBody] SubmitAnswerRequest request)
        {
            // Basic input validation
            if (request == null)
            {
                Console.WriteLine("Submit answer received null request");
                return BadRequest("Invalid request data.");
            }

            Console.WriteLine($"Processing answer submission - QuizId: {quizId}, PlayerId: {request.PlayerId}, QuestionId: {request.QuestionId}, AnswerIndex: {request.AnswerIndex}");

            try
            {
                // Get the question by position instead of ID
                var question = _dbService.GetQuizQuestion(quizId, request.QuestionId);
                if (question == null)
                {
                    Console.WriteLine($"Question not found for quiz {quizId}, position {request.QuestionId}");
                    return NotFound("Question not found.");
                }

                // Make sure the answer index is valid
                if (request.AnswerIndex < 0 || request.AnswerIndex >= question.Answers.Count())
                {
                    Console.WriteLine($"Invalid answer index {request.AnswerIndex}. Available answers: {question.Answers.Count()}");
                    return BadRequest("Invalid answer index.");
                }

                // Find the player record
                var playerRecord = _dbService.GetQuizRecord(request.PlayerId);
                if (playerRecord == null)
                {
                    Console.WriteLine($"Player record not found for ID {request.PlayerId}");
                    return NotFound("Player record not found.");
                }

                // Debug logging
                Console.WriteLine($"Processing answer for player {playerRecord.PlayerName}, question {question.QuestionText}");
                Console.WriteLine($"Selected answer index: {request.AnswerIndex}");

                // Determine if the answer is correct - find the index of the correct answer
                int correctAnswerIndex = -1;
                for (int i = 0; i < question.Answers.Count(); i++)
                {
                    if (question.Answers.ElementAt(i).Correct)
                    {
                        correctAnswerIndex = i;
                        break;
                    }
                }

                Console.WriteLine($"Correct answer index: {correctAnswerIndex}");
                bool isCorrect = correctAnswerIndex == request.AnswerIndex;
                Console.WriteLine($"Answer is correct: {isCorrect}");

                // Calculate score based on time remaining
                int calculatedScore = 0;
                if (isCorrect)
                {
                    int maxPoints = question.QuestionMaxPoints;
                    calculatedScore = (int)(maxPoints * (0.5 + (request.TimeRemaining / (double)question.QuestionTime) * 0.5));
                    Console.WriteLine($"Calculated score: {calculatedScore} (max points: {maxPoints}, time remaining: {request.TimeRemaining})");
                }

                // Update player score
                playerRecord.Score = playerRecord.Score.HasValue ? playerRecord.Score.Value + calculatedScore : calculatedScore;
                Console.WriteLine($"Updated total score: {playerRecord.Score}");
                _dbService.UpdateQuizRecord(playerRecord);

                // Return score and correct answer
                return Ok(new
                {
                    isCorrect = isCorrect,
                    score = calculatedScore,
                    totalScore = playerRecord.Score,
                    correctAnswerIndex = correctAnswerIndex
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SubmitAnswer: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while processing the answer.");
            }
        }



        // GET: api/Quiz/player/5/results/2
        [HttpGet("player/{playerId}/results/{quizId}")]
        public ActionResult<object> GetQuizResults(int playerId, int quizId)
        {
            var playerRecord = _dbService.GetQuizRecord(playerId);
            if (playerRecord == null)
            {
                return NotFound("Player record not found.");
            }

            var quiz = _dbService.GetQuizById(quizId);
            if (quiz == null)
            {
                return NotFound("Quiz not found.");
            }

            return Ok(new
            {
                quizTitle = quiz.Title,
                playerName = playerRecord.PlayerName,
                score = playerRecord.Score,
                totalQuestions = quiz.Questions.Count(),
                sessionId = playerRecord.SessionId
            });
        }

        // POST: api/Quiz
        [HttpPost]
        public ActionResult<FullQuizDTO> PostQuiz(FullQuizDTO quizDTO)
        {
            var quiz = _dbService.AddFullQuiz(quizDTO);
            if (quiz == null)
            {
                return BadRequest("Invalid quiz data.");
            }

            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.QuizId }, quiz);
        }

        // PUT: api/Quiz/5
        [HttpPut("{id}")]
        public IActionResult PutQuiz(int id, FullQuizDTO quizDTO)
        {
            if (id != quizDTO.QuizId)
            {
                return BadRequest();
            }

            var updatedQuiz = _dbService.UpdateFullQuiz(quizDTO);
            if (updatedQuiz == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Quiz/5
        [HttpDelete("{id}")]
        public ActionResult<QuizDTO> DeleteQuiz(int id)
        {
            var quiz = _dbService.DeleteQuiz(id);
            if (quiz == null)
            {
                return NotFound();
            }
            return Ok(quiz);
        }
    }

    // Request DTOs for the new endpoints
    public class StartQuizRequest
    {
        public string PlayerName { get; set; }
        public int QuizId { get; set; }
    }

    public class SubmitAnswerRequest
    {
        public int PlayerId { get; set; }
        public int QuestionId { get; set; }
        public int AnswerIndex { get; set; }
        public int TimeRemaining { get; set; }
    }
}
