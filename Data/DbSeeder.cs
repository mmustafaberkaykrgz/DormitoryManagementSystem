using Microsoft.EntityFrameworkCore;
using DormitoryManagementSystem.Models;

namespace DormitoryManagementSystem.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Apply all pending migrations to ensure database schema is up to date
            context.Database.Migrate();

            // If no roles exist in the database, add the 3 main roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Staff" },
                    new Role { RoleName = "Student" }
                );
                context.SaveChanges();
            }

            // If no users exist, create an initial "Admin" account (Password: 12345)
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                if (adminRole != null)
                {
                    context.Users.Add(new User
                    {
                        Username = "admin",
                        PasswordHash = "12345", // NOTE: Passwords must be hashed (e.g., SHA256) in real projects!
                        RoleId = adminRole.Id,
                        IsActive = true
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}
