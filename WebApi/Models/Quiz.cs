using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public partial class Quiz
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int AuthorId { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; } = new List<Question>();

    public virtual ICollection<QuizHistory> QuizHistories { get; } = new List<QuizHistory>();

    public virtual ICollection<QuizRecord> QuizRecords { get; } = new List<QuizRecord>();

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        return Equals(Id, (obj as Quiz).Id);
    }
}
