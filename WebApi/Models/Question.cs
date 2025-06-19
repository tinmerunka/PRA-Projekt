using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Question
{
    public int Id { get; set; }

    public string QuestionText { get; set; } = null!;

    public int QuizId { get; set; }

    public int QuestionPosition { get; set; }

    public int QuestionTime { get; set; }

    public int QuestionMaxPoints { get; set; }

    public virtual ICollection<Answer> Answers { get; } = new List<Answer>();

    public virtual Quiz Quiz { get; set; } = null!;
}
