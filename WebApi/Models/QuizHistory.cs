using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public partial class QuizHistory
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int QuizId { get; set; }

    public int? WinnerId { get; set; }

    public DateTime PlayedAt { get; set; }

    public string WinnerName { get; set; } = null!;

    public int WinnerScore { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User? Winner { get; set; }
}
