using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Persistence.Context;
using AstroConnect.Persistence.Identity;
using AstroConnect.Persistence.Repositories;
using AstroConnect.Persistence.SeedData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // =========================================================
            // DATABASE
            // =========================================================

            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string 'DefaultConnection' was not found.");
            }

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));


            // =========================================================
            // REPOSITORIES
            // =========================================================

            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();

            builder.Services.AddScoped<IAstrologerRepository, AstrologerRepository>();

            builder.Services.AddScoped<IBookingRepository, BookingRepository>();

            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();


            // =========================================================
            // IDENTITY
            // =========================================================

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    // Password rules
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;

                    // Account protection
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(5);

                    // User settings
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            // =========================================================
            // AUTHENTICATION COOKIE
            // =========================================================

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";

                options.AccessDeniedPath = "/Account/AccessDenied";

                options.ExpireTimeSpan = TimeSpan.FromHours(8);

                options.SlidingExpiration = true;
            });


            // =========================================================
            // MVC
            // =========================================================

            builder.Services.AddControllersWithViews();


            var app = builder.Build();


            // =========================================================
            // HTTP PIPELINE
            // =========================================================

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");

                app.UseHsts();
            }


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();


            // =========================================================
            // STATIC ASSETS
            // =========================================================

            app.MapStaticAssets();


            // =========================================================
            // ROUTING
            // =========================================================

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Public}/{action=Home}/{id?}")
                .WithStaticAssets();


            // =========================================================
            // DATABASE / IDENTITY SEEDING
            // =========================================================

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context =
                        services.GetRequiredService<ApplicationDbContext>();

                    var roleManager =
                        services.GetRequiredService<RoleManager<IdentityRole>>();

                    var userManager =
                        services.GetRequiredService<UserManager<ApplicationUser>>();


                    // Apply pending EF Core migrations automatically.
                    await context.Database.MigrateAsync();


                    // Seed required Identity roles.
                    await IdentitySeeder.SeedRolesAsync(roleManager);


                    // Seed default admin account.
                    await IdentitySeeder.SeedAdminUserAsync(userManager);


                    // Seed sample/master data only in Development.
                    if (app.Environment.IsDevelopment())
                    {
                        await SeedData.InitializeAsync(context);
                    }
                }
                catch (Exception ex)
                {
                    var logger =
                        services.GetRequiredService<ILogger<Program>>();

                    logger.LogError(
                        ex,
                        "An error occurred while initializing the database.");

                    throw;
                }
            }


            app.Run();
        }
    }
}