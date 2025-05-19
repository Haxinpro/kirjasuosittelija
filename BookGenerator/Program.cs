using BookGenerator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookGenerator.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContent>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContent>()
    .AddDefaultTokenProviders();

builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContent>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure database is created
        context.Database.EnsureCreated();

        Console.WriteLine("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
    }
}

app.UseStaticFiles();  // Lisää tämä varmistaaksesi, että CSS ladataan oikein
app.UseRouting();
app.UseAuthentication(); // Add this line
app.UseAuthorization();  // Add this line
app.MapRazorPages();
app.Run();