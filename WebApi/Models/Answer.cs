using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Answer
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public bool Correct { get; set; }

    public virtual Question Question { get; set; } = null!;
}
