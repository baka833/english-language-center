using EnglishCenter.API.Models;
using EnglishCenter.API.Services.Impl;
using EnglishCenter.API.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add controllers
            builder.Services.AddControllers();

            // Add database
            builder.Services.AddDbContext<EnglishCenterDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

            // Add services to the container.
            builder.Services.AddScoped<IAdminManagementService, AdminManagementService>();
            builder.Services.AddScoped<ITeacherManagementService, TeacherManagementService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Add CORS for web client
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWebClient", policy =>
                    policy.WithOrigins("https://localhost:7197")  // Web URL
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowWebClient");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
