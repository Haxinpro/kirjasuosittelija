using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BookGenerator.Data;
using System.Linq;

namespace BookGenerator.Pages
{
    public class DeleteAccountModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContent _db;

        public DeleteAccountModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContent db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // 1. Poista käyttäjän kirjat
                var userBooks = await _db.Book
                    .Where(b => b.UserId == user.Id)
                    .ToListAsync();

                if (userBooks.Any())
                {
                    _db.Book.RemoveRange(userBooks);
                    await _db.SaveChangesAsync();
                }

                // 2. Kirjaa ulos ja poista käyttäjä
                await _signInManager.SignOutAsync();
                await _userManager.DeleteAsync(user);
            }

            return RedirectToPage("/Index");
        }
    }
}
