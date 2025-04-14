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
        public DbSet<Best_Solution> bestsolutions { get; set; }
         public DbSet<Languages> languages { get; set; }
         public DbSet<Ai_Answer_Boot> ai_answer_boots { get; set; }
        public DbSet<Ai_Feedback> ai_feedbacks { get; set; }
        public DbSet<Users_Ai> users_ai { get; set; }
        public DbSet<UserProblem> Users_Problems { get; set; }
        public DbSet<TestCases> testCases { get; set; }
        public DbSet<AdminRequest> adminRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //1- علاقة one-to-one بين Problem و Best_Solution
            modelBuilder.Entity<Problem>()
                .HasOne(p => p.best_Solution)
                .WithOne(b => b.Problem)
                .HasForeignKey<Best_Solution>(b => b.ProgramId);

            //2- علاقة one-to-many بين Languages و  Best_Solution
            modelBuilder.Entity<Languages>()
                .HasMany(b => b.Best_Solution)
                .WithOne(l => l.languages)
                .HasForeignKey(l => l.languageId);

            modelBuilder.Entity<Users>()
              .HasMany(b => b.AdminRequests)
              .WithOne(l => l.User)
              .HasForeignKey(l => l.UserId);
            //3- علاقة one-to-many  بين  Problems و TestCases
            modelBuilder.Entity<Problem>()
                .HasMany(tc => tc.testCases)
                .WithOne(p => p.problems)
                .HasForeignKey(p => p.problemId);

            // علاقة Many-to-Many بين Users و Ai_Answer_Boot عبر Users_Ai

            modelBuilder.Entity<Users_Ai>()
             .HasKey(up => new { up.UserId, up.AiAnswerId });

            modelBuilder.Entity<Users_Ai>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.users)
                .HasForeignKey(ua => ua.UserId);

            modelBuilder.Entity<Users_Ai>()
                .HasOne(ua => ua.AiAnswer)
                .WithMany(a => a.users)
                .HasForeignKey(ua => ua.AiAnswerId);

            // علاقة Many-to-Many بين Users و Problem
            modelBuilder.Entity<UserProblem>()
                .HasKey(up => new { up.UserId, up.ProblemId });  // تعيين المفتاح المركب

            modelBuilder.Entity<Users>()
                .HasMany(u => u.UserProblems)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<Problem>()
                .HasMany(p => p.UserProblems)
                .WithOne(up => up.Problem)
                .HasForeignKey(up => up.ProblemId);

            //8- علاقة one-to-one بين UserProblem و Submission
            modelBuilder.Entity<UserProblem>()
           .HasKey(up => new { up.UserId, up.ProblemId });  // تحديد المفتاح المركب في UserProblem

            // العلاقة بين Submission و UserProblem (One-to-One)
            modelBuilder.Entity<UserProblem>()
            .HasKey(up => new { up.UserId, up.ProblemId });

            // تعريف العلاقة One-to-One بين Submission و UserProblem
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.userProblem)  // Submission يرتبط بـ UserProblem
                .WithOne(up => up.submission)  // UserProblem يحتوي على Submission
                .HasForeignKey<Submission>(s => new { s.UserId, s.ProblemId });

            //9- علاقة one-to-many بين Languages و Submission
            modelBuilder.Entity<Languages>()
                .HasMany<Submission>()
                .WithOne(s => s.Languages)
                .HasForeignKey(s => s.LanguagesId);

            //10- علاقة one-to-one بين Submission و Ai_Feedback
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Ai_Feedback)
                .WithOne(af => af.Submission)
                .HasForeignKey<Ai_Feedback>(af => af.SubmissionId);
        }



    }

}
