
using HrmsApi.Model;
using HrmsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HrmsApi.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options) 
        {
        
        }

        public DbSet<userLogin> userLogin { get; set; }

        public DbSet<JoiningForm> JoiningForm { get; set; }

        //public DbSet<TicketDetails> Tickets { get; set; }

        public DbSet<f16> f16 { get; set; }


        //------offfer ---
        public DbSet<OfferLetter> OfferLetters { get; set; }


        //-- leave ---- 
        public DbSet<LeaveRequest> LeaveRequest { get; set; }
        public DbSet<DetailsForLeave> DetailsForLeave { get; set; }



        //------task ---------
        public DbSet<NewTask> NewTasks { get; set; }
        public DbSet<User> Users { get; set; }



        //testing
        public DbSet<StudentBatch> StudentBatches { get; set; }
        public DbSet<Student> Students { get; set; }



        //submit task
        public DbSet<StudentTask> StudentTasks { get; set; }
        public DbSet<SubmittedTask> SubmittedTasks { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<userLogin>()
            //    .HasMany<JoiningForm>()
            //    .WithOne(j => j.userLogin)
            //    .HasForeignKey(j => j.Uid);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StudentBatch>()
             .HasKey(sb => sb.ID);

            modelBuilder.Entity<Student>()
                .HasKey(s => s.ID);


            modelBuilder.Entity<Student>()
                .HasOne(s => s.StudentBatch)
                .WithMany(sb => sb.Students)
                .HasForeignKey(s => s.StudentBatchId);


            //--------------payslip-------------------------

            //modelBuilder.Entity<userLogin>()
            //    .HasAlternateKey(a => a.uemail); // Configure email as alternate key

            //modelBuilder.Entity<LeaveRequest>()
            //    .HasOne(l => l.userlogin)
            //    .WithMany()
            //    .HasForeignKey(l => l.uemail)
            //    .HasPrincipalKey(a => a.uemail); // Use email as principal key

            //modelBuilder.Entity<JoiningForm>()
            //    .HasOne(j => j.userLogin)
            //    .WithMany()
            //    .HasForeignKey(j => j.uemail)
            //    .HasPrincipalKey(a => a.uemail); // Use email as principal key

            //modelBuilder.Entity<OfferLetter>()
            //    .HasOne(o => o.userLogin)
            //    .WithMany()
            //    .HasForeignKey(o => o.uemail)
            //    .HasPrincipalKey(a => a.uemail);

        }

    }
}
