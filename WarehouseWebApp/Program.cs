
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WarehouseWebApp.Data;
using WarehouseWebApp.Entities;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

var services = builder.Services;


var server = Environment.GetEnvironmentVariable("DB_HOST");
var database = Environment.GetEnvironmentVariable("DB_DATABASE");
var user = Environment.GetEnvironmentVariable("DB_USER");
var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
var port = Environment.GetEnvironmentVariable("DB_PORT");
var connectionString = $"Server={server};Database={database};User={user};Password={password};Port={port}";
var serverVersion = new MySqlServerVersion(new Version(8, 0));
services.AddDbContext<DataContext>(options =>
{
    options.UseMySql(connectionString ?? throw new InvalidOperationException(), serverVersion);
});


// Add services to the container.
services.AddControllersWithViews();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();


services.AddAuthorization();
services.AddAuthentication()
    .AddCookie(IdentityConstants.ApplicationScheme, options => 
    {
        options.LoginPath = new PathString("/Account/Login");
        
        options.Events.OnSigningIn = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var user = await userManager.GetUserAsync(context.Principal);

            if (user != null)
            {
                user.lastLogin = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
            }
        };
    });

services.AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddApiEndpoints();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var service = scope.ServiceProvider;
    var context = service.GetRequiredService<DataContext>();
    context.Database.Migrate();

    var seeder = new DataSeeder(
        service.GetRequiredService<UserManager<User>>(),
        service.GetRequiredService<RoleManager<IdentityRole>>());
    await seeder.seedRoles();
    await seeder.SeedAdminUser();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<User>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();