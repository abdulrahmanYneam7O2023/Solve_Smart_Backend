using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Solve_Smart_Backend.DDL.Models;

namespace Solve_Smart_Backend.DDL.Context
{
    public class Solvedbcontext : IdentityDbContext<Users>
    {
        public Solvedbcontext(DbContextOptions<Solvedbcontext> options) : base(options)
        {
        }
       public DbSet<Users> users { get; set; }
        public DbSet<Problem> problems { get; set; }
        public DbSet<Submission> submissions { get; set; }
      
          public DbSet<Ai_Feedback> ai_feedbacks { get; set; }
         public DbSet<Languages> languages { get; set; }
        public DbSet<UserProblem> Users_Problems { get; set; }
     
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<UserProblem>()
          .HasKey(up => up.Id);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.UserProblems)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<Problem>()
                .HasMany(p => p.UserProblems)
                .WithOne(up => up.Problem)
                .HasForeignKey(up => up.ProblemId);

            // علاقة One-to-Many بين UserProblem و Submission
            modelBuilder.Entity<UserProblem>()
             .HasMany(up => up.Submissions)
             .WithOne(s => s.userProblem)
             .HasForeignKey(s => s.UserProblemId);

            // علاقة One-to-Many بين Languages و Submission
            modelBuilder.Entity<Languages>()
                .HasMany<Submission>()
                .WithOne(s => s.Languages)
                .HasForeignKey(s => s.LanguagesId);

            // علاقة One-to-One بين Submission و Ai_Feedback
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.AiFeedback)
                .WithOne(af => af.Submission)
                .HasForeignKey<Ai_Feedback>(af => af.SubmissionId);
        }



    }

}
