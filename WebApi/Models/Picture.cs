using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Picture
{
    public int Id { get; set; }

    public byte[] Image { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; } = new List<Question>();

    public virtual ICollection<User> Users { get; } = new List<User>();
}
