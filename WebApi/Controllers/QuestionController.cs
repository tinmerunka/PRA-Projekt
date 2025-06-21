using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Utilities;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IDbService _dbService;

        public QuestionController(IDbService dbService)
        {
            _dbService = dbService;
        }

        // POST: api/Question
        [HttpPost]
        public ActionResult<QuestionDTO> CreateQuestion([FromBody] QuestionDTO questionDTO)
        {
            try
            {
                if (questionDTO == null)
                {
                    return BadRequest("Question data is required");
                }

                // Validate answers
                if (questionDTO.Answers == null || !questionDTO.Answers.Any())
                {
                    return BadRequest("Question must include answers");
                }

               
                questionDTO.QuestionId = 0;

                var createdQuestion = _dbService.UpdateQuestion(questionDTO);
                if (createdQuestion == null)
                {
                    return BadRequest("Failed to create question");
                }

                return CreatedAtAction(nameof(GetQuestion), new { id = createdQuestion.QuestionId }, createdQuestion);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating question: " + ex.Message);
                return StatusCode(500, "Error creating question: " + ex.Message);
            }
        }

        // GET: api/Question/id
        [HttpGet("{id}")]
        public ActionResult<QuestionDTO> GetQuestion(int id)
        {
            var question = _dbService.GetQuestionById(id);
            if (question == null)
            {
                return NotFound("Question not found");
            }

            return Ok(question);
        }

        // PUT: api/Question/id
        [HttpPut("{id}")]
        public IActionResult UpdateQuestion(int id, [FromBody] QuestionDTO questionDTO)
        {
            try
            {
                if (questionDTO == null)
                {
                    return BadRequest("Question data is required");
                }

                if (id != questionDTO.QuestionId)
                {
                    return BadRequest("Question ID mismatch");
                }

                var updatedQuestion = _dbService.UpdateQuestion(questionDTO);
                if (updatedQuestion == null)
                {
                    return NotFound("Question not found");
                }

                return Ok(updatedQuestion); // Return the updated question instead of NoContent
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating question: " + ex.Message);
                return StatusCode(500, "Error updating question: " + ex.Message);
            }
        }


        // DELETE: api/Question/id
        [HttpDelete("{id}")]
        public ActionResult<QuestionDTO> DeleteQuestion(int id)
        {
            var question = _dbService.DeleteQuestion(id);
            if (question == null)
            {
                return NotFound("Question not found");
            }

            return Ok(question);
        }
    }
}
