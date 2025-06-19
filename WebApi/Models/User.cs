using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public partial class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime JoinDate { get; set; }

    public virtual ICollection<QuizHistory> QuizHistories { get; } = new List<QuizHistory>();

    public virtual ICollection<Quiz> Quizzes { get; } = new List<Quiz>();

    public bool Equals(User other)
    {
        if (other == null) return false;
        return (Id.Equals(other.Id));
    }
}
