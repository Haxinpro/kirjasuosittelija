// File: Data/AppDbContent.cs

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookGenerator.Models; // Ensure the models are still included for the Book class

namespace BookGenerator.Data // Change this to BookGenerator.Data
{
    public class AppDbContent : IdentityDbContext<ApplicationUser>
    {
        public AppDbContent(DbContextOptions<AppDbContent> options)
            : base(options)
        {
        }

        public DbSet<Book> Book { get; set; } // The DbSet for the Book entity
    }
}
