using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;
using Route = RaceDayAPI.Models.Route;
namespace RaceDayAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Route> Routes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users: unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Categories: unique name
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.CategoryName)
                .IsUnique();

            // Events -> Category
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Events -> Organiser
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrolments: unique participant+event
            modelBuilder.Entity<Enrolment>()
                .HasIndex(en => new { en.ParticipantID, en.EventID })
                .IsUnique();

            // Enrolments -> Participant
            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Participant)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(en => en.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrolments -> Event
            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Event)
                .WithMany(e => e.Enrolments)
                .HasForeignKey(en => en.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            // Results: one per enrolment
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentID)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // Routes: one per event
            modelBuilder.Entity<Route>()
                .HasIndex(r => r.EventID)
                .IsUnique();

            modelBuilder.Entity<Route>()
                .HasOne(r => r.Event)
                .WithOne(e => e.Route)
                .HasForeignKey<Route>(r => r.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal precision
            modelBuilder.Entity<Event>()
                .Property(e => e.Distance)
                .HasColumnType("decimal(6,2)");

            modelBuilder.Entity<Route>()
                .Property(r => r.ElevationGain)
                .HasColumnType("decimal(6,2)");

            // Seed categories only (no Users/Events in seed — passwords must be hashed at runtime)
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, CategoryName = "Road Running",  Description = "Running events on tarred roads, ranging from 5km to full marathon" },
                new Category { CategoryID = 2, CategoryName = "Cycling",       Description = "Road and trail cycling events for all fitness levels" },
                new Category { CategoryID = 3, CategoryName = "Walking",       Description = "Casual and competitive walking events, great for all ages" },
                new Category { CategoryID = 4, CategoryName = "Trail Running", Description = "Off-road running on natural terrain and mountain paths" }
            );
        }
    }
}
