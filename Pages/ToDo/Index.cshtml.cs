using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;

namespace ToDoApp.Pages.ToDo
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public ToDoItem NewItem { get; set; }

        public List<ToDoItem> UserItems { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            UserItems = await _context.ToDoItems
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"NOUVELLE TÂCHE : {NewItem?.Title}");

            //if (!ModelState.IsValid) return await OnGetAsync();
            // Test temporaire : forcer l'enregistrement même si ModelState invalide
            // ATTENTION : à ne pas laisser en production
            if (string.IsNullOrWhiteSpace(NewItem?.Title))
            {
                return await OnGetAsync(); // rien à faire
            }

            NewItem.UserId = _userManager.GetUserId(User);
            NewItem.CreatedAt = DateTime.UtcNow;

            _context.ToDoItems.Add(NewItem);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var item = await _context.ToDoItems.FindAsync(id);
            if (item == null || item.UserId != _userManager.GetUserId(User))
                return NotFound();

            item.IsCompleted = !item.IsCompleted;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var item = await _context.ToDoItems.FindAsync(id);
            if (item == null || item.UserId != _userManager.GetUserId(User))
                return NotFound();

            _context.ToDoItems.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
