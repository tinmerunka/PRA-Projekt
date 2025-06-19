using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class QuizRecord
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string SessionId { get; set; } = null!;

    public string PlayerName { get; set; } = null!;

    public int? Score { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;
}
