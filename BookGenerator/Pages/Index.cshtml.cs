using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using BookGenerator.Data;
using BookGenerator.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookGenerator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContent _db;
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(AppDbContent db, ILogger<IndexModel> logger, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public Book? NewBook { get; set; }

        public void OnGet()
        {
            // Optional logic for GET requests (e.g. initial load)
        }

        // Handler for traditional form-based post (not used in this case but still included)
        public async Task<IActionResult> OnPostAddToLibraryAsync()
        {
            if (ModelState.IsValid)
            {
                _db.Book.Add(NewBook);
                await _db.SaveChangesAsync();
                return RedirectToPage("Library");
            }

            return Page();
        }

        // Called by JavaScript to save a book using fetch
        public async Task<IActionResult> OnPostAddBookAsync([FromBody] Book newBook)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Invalid data: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not logged in" });
            }

            Console.WriteLine($"Adding book: {newBook.Title} for user: {user.Id}");

            newBook.UserId = user.Id;
            await _db.Book.AddAsync(newBook);
            await _db.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        //Fetches distinct authors from user's saved books
        [HttpGet]
        public async Task<IActionResult> OnGetMyAuthorsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new List<string>()); // Empty if not logged in
            }

            var authors = await _db.Book
                .Where(b => b.UserId == user.Id && !string.IsNullOrEmpty(b.Author))
                .Select(b => b.Author)
                .Distinct()
                .ToListAsync();

            return new JsonResult(authors);
        }
    }
}
