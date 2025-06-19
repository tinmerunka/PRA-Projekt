using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs;
using WebApi.Models;

namespace WebApi.Utilities
{
    public interface IDbService {
        public UserDTO? GetUserByUsername(string username);
        public IEnumerable<UserDTO> GetUsers();
        public FullQuizDTO AddFullQuiz(FullQuizDTO fullQuizDTO);
        public FullQuizDTO? UpdateFullQuiz(FullQuizDTO fullQuizDTO);
        public QuestionDTO? GetQuizQuestion(int quizId, int questionNumber);
        public IEnumerable<QuizDTO> GetAllQuizzes();
        public IEnumerable<QuizDTO> GetQuizzesByAuthorId(int AuthorId);
        public FullQuizDTO? GetQuizById(int QuizId);
        public QuizDTO? DeleteQuiz(int QuizId);
        public IEnumerable<QuizHistoryDTO>? GetAllQuizHistory();
        public QuizHistoryDTO? GetQuizHistory(int id);
        public void AddQuizHistory(QuizHistoryDTO quizHistoryDTO);
        public IEnumerable<QuizHistoryDTO>? GetQuizHistoryByAuthorId(int authorId);
        public IEnumerable<QuizHistoryDTO> GetQuizHistoryByQuizId(int quizId);
        public QuizHistoryDTO? DeleteQuizHistory(int id);
        public UserDTO? GetUser(int userId);
        public int RegisterUser(UserRegisterDTO user); 
        public bool LoginUser(UserLoginDTO user);
        public UserDTO? DeleteUser(int userId);
        public bool UpdateUserPassword(UserChangePasswordDTO user);
        public bool UpdateUserInfo(UserDTO newUser);
        public QuizRecordDTO? GetQuizRecord(int id);
        public IEnumerable<QuizRecordDTO>? GetQuizRecordsBySessionCode(string sessionCode);
        public int AddNewPlayer(QuizRecordDTO quizRecordDTO);
        public void UpdateQuizRecord(QuizRecordDTO quizRecord);
        public QuestionDTO? GetQuestionById(int questionId);
        public QuestionDTO? DeleteQuestion(int questionId);
        public QuestionDTO? UpdateQuestion(QuestionDTO questionDTO);

    }

    public class DbServices : IDbService
    {
        PraContext _praContext;
        IMapper _mapper;
        public DbServices(PraContext praContext, IMapper mapper) { 
            _praContext = praContext;
            _mapper = mapper;
        }

        public IEnumerable<UserDTO> GetUsers() {
            return _mapper.Map<IEnumerable<UserDTO>>(_praContext.Users);
        }

        public FullQuizDTO? AddFullQuiz(FullQuizDTO fullQuizDTO)
        {
            try
            {
                // Validation with specific error messages
                if (fullQuizDTO.Questions.Count() > 20 || fullQuizDTO.Questions.Count() < 1)
                {
                    Console.WriteLine($"Quiz validation failed: Question count is {fullQuizDTO.Questions.Count()}, must be between 1 and 20");
                    return null;
                }

                foreach (var question in fullQuizDTO.Questions)
                {
                    if (question.Answers.Count() != 4)
                    {
                        Console.WriteLine($"Quiz validation failed: Question {question.QuestionPosition} has {question.Answers.Count()} answers, must be exactly 4");
                        return null;
                    }
                    if (question.QuestionTime < 15 || question.QuestionTime > 60)
                    {
                        Console.WriteLine($"Quiz validation failed: Question {question.QuestionPosition} has time {question.QuestionTime}, must be between 15 and 60");
                        return null;
                    }
                    if (question.Answers.Count(x => x.Correct) != 1)
                    {
                        Console.WriteLine($"Quiz validation failed: Question {question.QuestionPosition} has {question.Answers.Count(x => x.Correct)} correct answers, must be exactly 1");
                        return null;
                    }
                }

                var fullQuiz = _mapper.Map<Quiz>(fullQuizDTO);

                // Execute database operation with detailed error catching
                _praContext.Quizzes.Add(fullQuiz);
                _praContext.SaveChanges();

                fullQuizDTO.QuizId = fullQuiz.Id;
                return fullQuizDTO;
            }
            catch (DbUpdateException dbEx)
            {
                // Check for unique constraint violation
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("UQ__Quizzes__2CB664DC"))
                {
                    Console.WriteLine($"Database error: Quiz title '{fullQuizDTO.Title}' already exists. Titles must be unique.");
                }
                else
                {
                    Console.WriteLine($"Database error: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error saving quiz: {ex.Message}");
                return null;
            }
        }


        public FullQuizDTO? UpdateFullQuiz(FullQuizDTO fullQuizDTO)
        {
            try
            {
                // Validation with specific error messages - similar to AddFullQuiz
                if (fullQuizDTO.Questions.Count() > 20 || fullQuizDTO.Questions.Count() < 1)
                {
                    Console.WriteLine($"Quiz validation failed: Question count is {fullQuizDTO.Questions.Count()}, must be between 1 and 20");
                    return null;
                }

                foreach (var question in fullQuizDTO.Questions)
                {
                    if (question.Answers.Count() != 4)
                    {
                        Console.WriteLine($"Quiz validation failed: Question {question.QuestionPosition} has {question.Answers.Count()} answers, must be exactly 4");
                        return null;
                    }
                    if (question.QuestionTime < 15 || question.QuestionTime > 60)
                    {
                        Console.WriteLine($"Quiz validation failed: Question {question.QuestionPosition} has time {question.QuestionTime}, must be between 15 and 60");
                        return null;
                    }
                    if (question.Answers.Count(x => x.Correct) != 1)
                    {
                        Console.WriteLine($"Quiz validation failed: Question {question.QuestionPosition} has {question.Answers.Count(x => x.Correct)} correct answers, must be exactly 1");
                        return null;
                    }
                }

                // Get existing quiz
                var existingQuiz = _praContext.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefault(x => x.Id == fullQuizDTO.QuizId);

                if (existingQuiz == null)
                {
                    return null;
                }

                // Update basic quiz properties
                existingQuiz.Title = fullQuizDTO.Title;
                existingQuiz.Description = fullQuizDTO.Description;

                // Process questions
                // First, get a list of questions to keep, update, or add
                var existingQuestionIds = existingQuiz.Questions.Select(q => q.Id).ToList();
                var updatedQuestionIds = fullQuizDTO.Questions.Where(q => q.QuestionId != 0).Select(q => q.QuestionId).ToList();

                // Find questions to delete (in existing but not in updated)
                var questionsToDelete = existingQuiz.Questions
                    .Where(q => !updatedQuestionIds.Contains(q.Id))
                    .ToList();

                // Delete questions not in the updated list
                foreach (var questionToDelete in questionsToDelete)
                {
                    // Delete answers first
                    foreach (var answer in questionToDelete.Answers.ToList())
                    {
                        _praContext.Answers.Remove(answer);
                    }
                    _praContext.Questions.Remove(questionToDelete);
                }

                // Process each question from the DTO
                foreach (var questionDTO in fullQuizDTO.Questions)
                {
                    if (questionDTO.QuestionId == 0)
                    {
                        // This is a new question
                        var newQuestion = new Question
                        {
                            QuizId = existingQuiz.Id,
                            QuestionText = questionDTO.QuestionText,
                            QuestionPosition = questionDTO.QuestionPosition,
                            QuestionTime = questionDTO.QuestionTime,
                            QuestionMaxPoints = questionDTO.QuestionMaxPoints
                        };

                        // Add new answers
                        foreach (var answerDTO in questionDTO.Answers)
                        {
                            var newAnswer = new Answer
                            {
                                AnswerText = answerDTO.AnswerText,
                                Correct = answerDTO.Correct
                            };
                            newQuestion.Answers.Add(newAnswer);
                        }

                        _praContext.Questions.Add(newQuestion);
                    }
                    else
                    {
                        // Update existing question
                        var existingQuestion = existingQuiz.Questions
                            .FirstOrDefault(q => q.Id == questionDTO.QuestionId);

                        if (existingQuestion != null)
                        {
                            // Update question properties
                            existingQuestion.QuestionText = questionDTO.QuestionText;
                            existingQuestion.QuestionPosition = questionDTO.QuestionPosition;
                            existingQuestion.QuestionTime = questionDTO.QuestionTime;
                            existingQuestion.QuestionMaxPoints = questionDTO.QuestionMaxPoints;

                            // Get existing answer IDs
                            var existingAnswerIds = existingQuestion.Answers.Select(a => a.Id).ToList();
                            var updatedAnswerIds = questionDTO.Answers
                                .Where(a => a.AnswerId != 0)
                                .Select(a => a.AnswerId)
                                .ToList();

                            // Process each answer
                            foreach (var answerDTO in questionDTO.Answers)
                            {
                                if (answerDTO.AnswerId == 0)
                                {
                                    // Add new answer
                                    var newAnswer = new Answer
                                    {
                                        QuestionId = existingQuestion.Id,
                                        AnswerText = answerDTO.AnswerText,
                                        Correct = answerDTO.Correct
                                    };
                                    _praContext.Answers.Add(newAnswer);
                                }
                                else
                                {
                                    // Update existing answer
                                    var existingAnswer = existingQuestion.Answers
                                        .FirstOrDefault(a => a.Id == answerDTO.AnswerId);

                                    if (existingAnswer != null)
                                    {
                                        existingAnswer.AnswerText = answerDTO.AnswerText;
                                        existingAnswer.Correct = answerDTO.Correct;
                                    }
                                    else
                                    {
                                        // Answer ID was sent but doesn't exist, create new
                                        var newAnswer = new Answer
                                        {
                                            QuestionId = existingQuestion.Id,
                                            AnswerText = answerDTO.AnswerText,
                                            Correct = answerDTO.Correct
                                        };
                                        _praContext.Answers.Add(newAnswer);
                                    }
                                }
                            }

                            // Delete answers that are in existing but not in updated
                            foreach (var answer in existingQuestion.Answers.ToList())
                            {
                                if (!updatedAnswerIds.Contains(answer.Id))
                                {
                                    _praContext.Answers.Remove(answer);
                                }
                            }
                        }
                    }
                }

                _praContext.SaveChanges();
                return GetQuizById(fullQuizDTO.QuizId); // Return fresh data
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating quiz: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        public QuestionDTO? GetQuizQuestion(int quizId, int questionNumber) {
            Quiz? quiz = _praContext.Quizzes.Include("Questions").Include("Questions.Answers").FirstOrDefault(x => x.Id == quizId);
            if (quiz == null)
            {
                return null;
            }
            List<Question> questions = quiz.Questions.OrderBy(x => x.QuestionPosition).ToList();
            return _mapper.Map<QuestionDTO>(questions.ElementAt(questionNumber-1));
        }

        public IEnumerable<QuizDTO> GetAllQuizzes()
        {
            IEnumerable<Quiz> quizzes = _praContext.Quizzes.Include(q => q.Questions);
            List<QuizDTO> quizDTOs = new List<QuizDTO>();
            foreach (var quiz in quizzes)
            {
                quizDTOs.Add(_mapper.Map<QuizDTO>(quiz));
            }
            return quizDTOs;
        }


        public IEnumerable<QuizDTO> GetQuizzesByAuthorId(int AuthorId)
        {
            IEnumerable<Quiz> quizzes = _praContext.Quizzes
                .Include(q => q.Questions)
                .Where(x => x.AuthorId == AuthorId);
            IEnumerable<QuizDTO> quizDTOs = _mapper.Map<IEnumerable<QuizDTO>>(quizzes);
            return quizDTOs;
        }

        public FullQuizDTO? GetQuizById(int QuizId) {
            Quiz? quiz = _praContext.Quizzes.Include("Questions").Include("Questions.Answers").FirstOrDefault(x => x.Id == QuizId);

            if (quiz == null)
            {
                return null;
            }

            return _mapper.Map<FullQuizDTO>(quiz);
            
        }

        public QuizDTO? DeleteQuiz(int QuizId)
        {
            var existingQuiz = _praContext.Quizzes
                .Include("Questions")
                .Include("Questions.Answers")
                .Include("QuizRecords")  // Include QuizRecords to be able to delete them
                .FirstOrDefault(x => x.Id == QuizId);

            if (existingQuiz == null)
            {
                return null;
            }

            // First delete all related QuizRecords
            if (existingQuiz.QuizRecords != null && existingQuiz.QuizRecords.Any())
            {
                foreach (var record in existingQuiz.QuizRecords.ToList())
                {
                    _praContext.QuizRecords.Remove(record);
                }
            }

            // Then delete all QuizHistories
            if (_praContext.QuizHistories != null && _praContext.QuizHistories.Any())
            {
                IEnumerable<QuizHistory> quizHistories = _praContext.QuizHistories.Where(x => x.QuizId == QuizId);
                foreach (var quizHistory in quizHistories)
                {
                    _praContext.QuizHistories.Remove(quizHistory);
                }
            }

            // Now it's safe to delete the quiz
            var deletedQuizDto = _mapper.Map<QuizDTO>(existingQuiz);
            _praContext.Quizzes.Remove(existingQuiz);
            _praContext.SaveChanges();
            return deletedQuizDto;
        }



        public IEnumerable<QuizHistoryDTO> GetAllQuizHistory()
        {
            return _mapper.Map<IEnumerable<QuizHistoryDTO>>(_praContext.QuizHistories);
        }

        public QuizHistoryDTO? GetQuizHistory(int id) {
            QuizHistory? history = _praContext.QuizHistories.FirstOrDefault(x => x.Id == id);
            if (history == null)
            {
                return null;
            }
            return _mapper.Map<QuizHistoryDTO>(history);
        }

        public IEnumerable<QuizHistoryDTO>? GetQuizHistoryByAuthorId(int authorId) {
            IEnumerable<QuizHistory> histories = _praContext.QuizHistories.Include("Quiz").Where(x => x.Quiz.AuthorId == authorId);
            if (histories.Count() == 0)
            {
                return null;
            }
            return _mapper.Map<IEnumerable<QuizHistoryDTO>>(histories);
        }

        public void AddQuizHistory(QuizHistoryDTO quizHistoryDTO)
        {
            QuizHistory quizHistory = _mapper.Map<QuizHistory>(quizHistoryDTO);
            if (quizHistory.WinnerId == 0)
            {
                quizHistory.WinnerId = null;
            }
            _praContext.QuizHistories.Add(quizHistory);
            _praContext.SaveChanges();
        }

        public IEnumerable<QuizHistoryDTO> GetQuizHistoryByQuizId(int quizId)
        {
            IEnumerable<QuizHistory> histories = _praContext.QuizHistories.Where(x => x.QuizId == quizId);
            return _mapper.Map<IEnumerable<QuizHistoryDTO>>(histories);
        }

        public QuizHistoryDTO? DeleteQuizHistory(int id)
        {
            var quizHistory = _praContext.QuizHistories.FirstOrDefault(x => x.Id == id);
            if (quizHistory == null)
            {
                return null;
            }
            try
            {
                _praContext.Entry(quizHistory).State = EntityState.Detached;
            }
            catch (Exception){}
            _praContext.QuizHistories.Remove(quizHistory);
            _praContext.SaveChanges();
            return _mapper.Map<QuizHistoryDTO>(quizHistory);
        }

        public UserDTO? GetUser(int userId)
        {
            var user = _praContext.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                return null;
            }
            return _mapper.Map<UserDTO>(user);
        }

        public int RegisterUser(UserRegisterDTO user)
        {
            var newUser = _mapper.Map<User>(user);

            newUser.PasswordSalt = AuthUtilities.GetSalt();
            newUser.PasswordHash = AuthUtilities.GetStringSha256Hash(user.Password, newUser.PasswordSalt);
            newUser.JoinDate = DateTime.Now;

            _praContext.Users.Add(newUser);
            _praContext.SaveChanges();
            return newUser.Id;
        }

        public bool LoginUser(UserLoginDTO user)
        {
            var existingUser = _praContext.Users.FirstOrDefault(x => x.Username == user.Username);

            if (existingUser == null)
            {
                return false;
            }
            string v = AuthUtilities.GetStringSha256Hash(user.Password, existingUser.PasswordSalt);
            if (existingUser.PasswordHash != v)
            {
                return false;
            }
            return true;
        }

        public UserDTO? DeleteUser(int userId)
        {
            User? user = _praContext.Users.Include("Quizzes").FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                return null;
            }
            if (user.Quizzes != null)
            {
                foreach (var quiz in user.Quizzes)
                {
                    DeleteQuiz(quiz.Id);
                }
            }
            UserDTO userDto = _mapper.Map<UserDTO>(user);
            _praContext.Users.Remove(user);
            _praContext.SaveChanges();
            return userDto;
        }

        public bool UpdateUserPassword(UserChangePasswordDTO user) {
            var existingUser = _praContext.Users.FirstOrDefault(x => x.Username == user.Username);
            if (existingUser == null || AuthUtilities.GetStringSha256Hash(user.OldPassword, existingUser.PasswordSalt) != existingUser.PasswordHash)
            {
                return false;
            }

            existingUser.PasswordSalt = AuthUtilities.GetSalt();
            existingUser.PasswordHash = AuthUtilities.GetStringSha256Hash(user.NewPassword, existingUser.PasswordSalt);

            _praContext.Users.Update(existingUser);
            _praContext.SaveChanges();
            return true;
        }

        public bool UpdateUserInfo(UserDTO newUser) {
            var existingUser = _praContext.Users.FirstOrDefault(x => x.Id == newUser.Id);
            if (existingUser == null)
            {
                return false;
            }
            existingUser.Username = newUser.Username;
            existingUser.LastName = newUser.LastName;
            existingUser.FirstName = newUser.FirstName;
            existingUser.Email = newUser.Email;
            //_praContext.Entry(existingUser).State = EntityState.Detached;

            _praContext.Users.Update(existingUser);
            _praContext.SaveChanges();
            return true;
        }

        public int AddNewPlayer(QuizRecordDTO quizRecordDTO)
        {
            try
            {
                // Verify quiz exists first
                var quizExists = _praContext.Quizzes.Any(q => q.Id == quizRecordDTO.QuizId);
                if (!quizExists)
                {
                    Console.WriteLine($"Error in AddNewPlayer: Quiz with ID {quizRecordDTO.QuizId} does not exist");
                    return -1;
                }

                // Ensure required fields are provided
                if (string.IsNullOrEmpty(quizRecordDTO.PlayerName))
                {
                    Console.WriteLine("Error in AddNewPlayer: PlayerName is required");
                    return -1;
                }

                if (string.IsNullOrEmpty(quizRecordDTO.SessionId))
                {
                    Console.WriteLine("Error in AddNewPlayer: SessionId is required");
                    return -1;
                }

                if (quizRecordDTO.SessionId.Length != 6) // Check against database constraint
                {
                    Console.WriteLine($"Warning: SessionId length {quizRecordDTO.SessionId.Length} doesn't match database constraint of 6");
                    // Truncate or pad the SessionId to exactly 6 characters
                    quizRecordDTO.SessionId = quizRecordDTO.SessionId.Length > 6
                        ? quizRecordDTO.SessionId.Substring(0, 6)
                        : quizRecordDTO.SessionId.PadRight(6);
                }

                quizRecordDTO.Id = 0;
                var newQuizRecord = _mapper.Map<QuizRecord>(quizRecordDTO);

                // Set default values for any nullable properties that might be causing issues
                if (newQuizRecord.Score == null)
                {
                    newQuizRecord.Score = 0;
                }

                _praContext.QuizRecords.Add(newQuizRecord);
                _praContext.SaveChanges();
                return newQuizRecord.Id;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error in AddNewPlayer: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                    Console.WriteLine($"Stack trace: {dbEx.InnerException.StackTrace}");
                }

                // Print the state entries for debugging
                foreach (var entry in dbEx.Entries)
                {
                    Console.WriteLine($"Entity of type {entry.Entity.GetType().Name} in state {entry.State}");
                }
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error in AddNewPlayer: {ex.Message}");
                return -1;
            }
        }


        public QuizRecordDTO? GetQuizRecord(int id) {
            var quizRecord = _praContext.QuizRecords.AsNoTracking().FirstOrDefault(x => x.Id == id);
            if (quizRecord == null)
            {
                return null;
            }
            _praContext.Entry(quizRecord).State = EntityState.Detached;
            return _mapper.Map<QuizRecordDTO>(quizRecord);
        }

        public QuestionDTO? GetQuestionById(int questionId)
        {
            var question = _praContext.Questions
                .Include(q => q.Answers)
                .FirstOrDefault(q => q.Id == questionId);

            if (question == null)
            {
                return null;
            }

            return _mapper.Map<QuestionDTO>(question);
        }

        public QuestionDTO? DeleteQuestion(int questionId)
        {
            var question = _praContext.Questions
                .Include(q => q.Answers)
                .FirstOrDefault(q => q.Id == questionId);

            if (question == null)
            {
                return null;
            }

            // First delete all related answers
            foreach (var answer in question.Answers.ToList())
            {
                _praContext.Answers.Remove(answer);
            }

            // Then delete the question itself
            var questionDTO = _mapper.Map<QuestionDTO>(question);
            _praContext.Questions.Remove(question);
            _praContext.SaveChanges();

            return questionDTO;
        }


        public QuestionDTO? UpdateQuestion(QuestionDTO questionDTO)
        {
            // First find the quiz without tracking
            var quiz = _praContext.Quizzes
                .Include(q => q.Questions)
                .Include("Questions.Answers")
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == questionDTO.QuizId);

            if (quiz == null)
            {
                return null;
            }

            // Find the question to update
            var question = _praContext.Questions
                .Include(q => q.Answers)
                .FirstOrDefault(q => q.Id == questionDTO.QuestionId);

            if (question == null)
            {
                // If question doesn't exist, it's a new question - add it to quiz
                var newQuestion = _mapper.Map<Question>(questionDTO);
                newQuestion.QuizId = questionDTO.QuizId;

                // Map answers
                foreach (var answer in questionDTO.Answers)
                {
                    var newAnswer = _mapper.Map<Answer>(answer);
                    newQuestion.Answers.Add(newAnswer);
                }

                _praContext.Questions.Add(newQuestion);
                _praContext.SaveChanges();
                return questionDTO;
            }
            else
            {
                // Update existing question
                question.QuestionText = questionDTO.QuestionText;
                question.QuestionTime = questionDTO.QuestionTime;
                question.QuestionMaxPoints = questionDTO.QuestionMaxPoints;
                question.QuestionPosition = questionDTO.QuestionPosition;

                // Remove old answers
                foreach (var oldAnswer in question.Answers.ToList())
                {
                    _praContext.Answers.Remove(oldAnswer);
                }

                // Add new answers
                foreach (var answerDTO in questionDTO.Answers)
                {
                    var answer = _mapper.Map<Answer>(answerDTO);
                    answer.QuestionId = question.Id;
                    _praContext.Answers.Add(answer);
                }

                _praContext.SaveChanges();
                return questionDTO;
            }
        }
        public IEnumerable<QuizRecordDTO>? GetQuizRecordsBySessionCode(string sessionCode)
        {
            var quizRecords = _praContext.QuizRecords.AsNoTracking().Where(x => x.SessionId == sessionCode);
            return _mapper.Map<IEnumerable<QuizRecordDTO>>(quizRecords);
        }

        public void UpdateQuizRecord(QuizRecordDTO quizRecordDTO)
        {
            try
            {
                // Using FindAsync for better performance
                var quizRecord = _praContext.QuizRecords.Find(quizRecordDTO.Id);
                if (quizRecord == null)
                {
                    Console.WriteLine($"ERROR: Quiz record with ID {quizRecordDTO.Id} not found");
                    return;
                }

                Console.WriteLine($"Updating score for player {quizRecord.PlayerName} from {quizRecord.Score} to {quizRecordDTO.Score}");
                quizRecord.Score = quizRecordDTO.Score;

                // Explicitly mark as modified
                _praContext.Entry(quizRecord).State = EntityState.Modified;
                _praContext.SaveChanges();

                Console.WriteLine($"Score update successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR updating quiz record: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }


        public UserDTO? GetUserByUsername(string username)
        {
            return GetUsers().FirstOrDefault(x => x.Username == username);
        }
    }
}
