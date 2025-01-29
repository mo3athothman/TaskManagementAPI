using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed default users (Admin and User)
            var admin = new User
            {
                Id = "1",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Administrator"
            };

            var user = new User
            {
                Id = "2",
                UserName = "user",
                NormalizedUserName = "USER",
                Email = "user@example.com",
                NormalizedEmail = "USER@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Regular User"
            };

            builder.Entity<User>().HasData(admin, user);

            builder.Entity<TaskItem>().HasData(
                new TaskItem { Id = 1, Title = "Task 1", Description = "First Task", Status = "Pending", AssignedUserId = "2" },
                new TaskItem { Id = 2, Title = "Task 2", Description = "Second Task", Status = "In Progress", AssignedUserId = "2" },
                new TaskItem { Id = 3, Title = "Task 3", Description = "Third Task", Status = "Completed", AssignedUserId = "2" }
            );
        }
    }
}