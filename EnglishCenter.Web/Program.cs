using EnglishCenter.Web.Services.Impl;
using EnglishCenter.Web.Services.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EnglishCenter.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Session stores raw JWT tokens after login; ApiAccessTokenHandler reads them to attach Bearer headers.
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromHours(8);
            });

            // Cookie auth wraps JWT claims in a server-side cookie; actual API calls use the stored token.
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.SlidingExpiration = true;
                });

            builder.Services.AddControllersWithViews();

            // ApiAccessTokenHandler injects the session JWT as Authorization: Bearer on every outgoing request.
            builder.Services.AddTransient<ApiAccessTokenHandler>();

            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            var apiUri = new Uri(apiBaseUrl.EndsWith('/') ? apiBaseUrl : $"{apiBaseUrl}/");

            // Auth client has no token handler — login/register endpoints are unauthenticated.
            builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(c => c.BaseAddress = apiUri);

            builder.Services.AddHttpClient<IAdminApiClient, AdminApiClient>(c => c.BaseAddress = apiUri)
                .AddHttpMessageHandler<ApiAccessTokenHandler>();

            builder.Services.AddHttpClient<ITeacherApiClient, TeacherApiClient>(c => c.BaseAddress = apiUri)
                .AddHttpMessageHandler<ApiAccessTokenHandler>();

            builder.Services.AddHttpClient<IStudentApiClient, StudentApiClient>(c => c.BaseAddress = apiUri)
                .AddHttpMessageHandler<ApiAccessTokenHandler>();

            builder.Services.AddHttpClient<IUserApiClient, UserApiClient>(c => c.BaseAddress = apiUri)
                .AddHttpMessageHandler<ApiAccessTokenHandler>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
