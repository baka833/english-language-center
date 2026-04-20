using EnglishCenter.API.Data;
using EnglishCenter.API.Models;
using EnglishCenter.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            builder.Services.AddDbContext<EnglishCenterDbContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

            // Add services to the container.

            // Add CORS for web client
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWebClient", policy =>
                    policy.WithOrigins("https://localhost:7197")  // Client URL
                          .AllowAnyHeader()
                          .AllowAnyMethod());

                //options.AddPolicy("AllowWebClient", 
                //    policy => policy.AllowAnyOrigin()
                //                    .AllowAnyHeader()
                //                    .AllowAnyMethod());
            });

            // JWT Authentication
            var jwtConfig = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters{
                        ValidateIssuer = true, 
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtConfig["Issuer"],
                        ValidAudience = jwtConfig["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

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

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            // Seed users
            using (var scope = app.Services.CreateScope())
                DataSeeder.SeedUsers(scope.ServiceProvider);

            app.Run();
        }
    }
}
