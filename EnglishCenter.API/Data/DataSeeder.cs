using EnglishCenter.API.Models;
using Microsoft.AspNetCore.Identity;

namespace EnglishCenter.API.Data
{
    public static class DataSeeder
    {
        public static void SeedUsers(IServiceProvider services)
        {
            var db = services.GetRequiredService<EnglishCenterDbContext>();
            var hasher = services.GetRequiredService<IPasswordHasher<User>>();

            var seeds = new[]
            {
                new { Username = "admin",    Password = "Admin@123",    Fullname = "System Admin",     Role = "Admin",   Gender = "Male",   Email = "admin@englishcenter.com",   Dob = new DateOnly(1985, 1, 1) },
                new { Username = "teacher1", Password = "Teacher@123",  Fullname = "Nguyen Van A",     Role = "Teacher", Gender = "Male",   Email = "teacher1@englishcenter.com", Dob = new DateOnly(1990, 5, 15) },
                new { Username = "teacher2", Password = "Teacher@123",  Fullname = "Tran Thi B",       Role = "Teacher", Gender = "Female", Email = "teacher2@englishcenter.com", Dob = new DateOnly(1992, 8, 20) },
                new { Username = "student1", Password = "Student@123",  Fullname = "Le Van C",         Role = "Student", Gender = "Male",   Email = "student1@example.com",       Dob = new DateOnly(2002, 3, 10) },
                new { Username = "student2", Password = "Student@123",  Fullname = "Pham Thi D",       Role = "Student", Gender = "Female", Email = "student2@example.com",       Dob = new DateOnly(2003, 7, 25) },
                new { Username = "student3", Password = "Student@123",  Fullname = "Hoang Van E",      Role = "Student", Gender = "Male",   Email = "student3@example.com",       Dob = new DateOnly(2001, 11, 5) },
            };

            var existingUsernames = db.Users
                .Select(user => user.Username)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var users = seeds
                .Where(seed => !existingUsernames.Contains(seed.Username))
                .Select(s =>
            {
                var user = new User
                {
                    Username = s.Username,
                    Fullname = s.Fullname,
                    Role = s.Role,
                    Gender = s.Gender,
                    Email = s.Email,
                    Dob = s.Dob,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = string.Empty
                };
                user.PasswordHash = hasher.HashPassword(user, s.Password);
                return user;
            }).ToList();

            if (users.Count == 0)
            {
                return;
            }

            db.Users.AddRange(users);
            db.SaveChanges();
        }
    }
}
