using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Models;

public partial class PraContext : DbContext
{
    public PraContext()
    {
    }

    public PraContext(DbContextOptions<PraContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizHistory> QuizHistories { get; set; }

    public virtual DbSet<QuizRecord> QuizRecords { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=ConnectionStrings:PRAcs");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Answers__3214EC276C191C8F");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AnswerText).HasMaxLength(150);
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__Answers__Questio__59C55456");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC2763EABC20");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.QuestionTime).HasDefaultValueSql("((30))");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK__Questions__QuizI__55F4C372");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Quizzes__3214EC27FAC62B69");

            entity.HasIndex(e => e.Title, "UQ__Quizzes__2CB664DC4C3A0D1D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.Title).HasMaxLength(75);

            entity.HasOne(d => d.Author).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quizzes__AuthorI__531856C7");
        });

        modelBuilder.Entity<QuizHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__QuizHist__3214EC271BB96CD9");

            entity.ToTable("QuizHistory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PlayedAt).HasColumnType("datetime");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.WinnerId).HasColumnName("WinnerID");
            entity.Property(e => e.WinnerName).HasMaxLength(75);

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizHistories)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuizHisto__QuizI__5D95E53A");

            entity.HasOne(d => d.Winner).WithMany(p => p.QuizHistories)
                .HasForeignKey(d => d.WinnerId)
                .HasConstraintName("FK__QuizHisto__Winne__5E8A0973");
        });

        modelBuilder.Entity<QuizRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__QuizReco__3214EC274CF09B2F");

            entity.ToTable("QuizRecord");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PlayerName).HasMaxLength(75);
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.Score).HasDefaultValueSql("((0))");
            entity.Property(e => e.SessionId)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizRecords)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuizRecor__QuizI__65370702");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC2773F6A01C");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E405C2550A").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.JoinDate).HasColumnType("date");
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.PasswordSalt).HasMaxLength(256);
            entity.Property(e => e.Username).HasMaxLength(75);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
