using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BookGenerator.Data;
using BookGenerator.Models;

namespace BookGenerator.Pages
{
    public class LibraryModel : PageModel
    {
        private readonly AppDbContent _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LibraryModel> _logger;
        public List<Book> Books { get; set; } = new List<Book>();
        public bool IsBookAdded { get; set; } = false;

        public LibraryModel(AppDbContent db, UserManager<ApplicationUser> userManager, ILogger<LibraryModel> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine("DEBUG: User is null - not logged in");
                return;
            }

            try
            {
                Books = await _db.Book
                    .Where(b => b.UserId == user.Id)
                    .ToListAsync();

                Console.WriteLine($"DEBUG: Found {Books.Count} books for user {user.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error fetching books: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostAddBookAsync([FromBody] Book newBook)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Model state is invalid");
                    return new JsonResult(new { success = false, message = "Invalid data" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("User not found");
                    return new JsonResult(new { success = false, message = "User not logged in" });
                }

                // 🔒 Tarkista onko käyttäjällä jo 5 kirjaa
                var currentCount = await _db.Book.CountAsync(b => b.UserId == user.Id);
                if (currentCount >= 5)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "You can only save up to 5 books. Please delete one before adding another."
                    });
                }

                newBook.UserId = user.Id;
                await _db.Book.AddAsync(newBook);
                await _db.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving book: {ex.Message}");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteBookAsync([FromBody] JsonElement data)
        {
            try
            {
                var bookId = data.GetProperty("bookId").GetString(); // Changed from GetInt32 to GetString
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "User not logged in" });
                }

                var book = await _db.Book.FindAsync(int.Parse(bookId)); // Parse the string to int
                if (book == null || book.UserId != user.Id)
                {
                    return new JsonResult(new { success = false, message = "Book not found" });
                }

                _db.Book.Remove(book);
                await _db.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting book: {ex.Message}");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
