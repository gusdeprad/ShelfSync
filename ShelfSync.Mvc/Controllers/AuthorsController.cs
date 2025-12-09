using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSync.Mvc.Data;
using ShelfSync.Mvc.Models.Entities;
using ShelfSync.Mvc.Models.ViewModels;

namespace ShelfSync.Mvc.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AuthorsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var authors = await _db.Authors
                                   .Include(a => a.Books)
                                   .AsNoTracking()
                                   .ToListAsync();

            var vms = authors.Select(a => a.ToViewModel());
            return View(vms);
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var author = await _db.Authors
                                  .Include(a => a.Books)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(a => a.Id == id.Value);

            if (author == null) return NotFound();

            return View(author.ToViewModel());
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Books"] = await _db.Books.AsNoTracking().ToListAsync();
            return View(new AuthorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuthorViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Books"] = await _db.Books.AsNoTracking().ToListAsync();
                return View(vm);
            }

            var bookIds = vm.BookIds ?? new List<Guid>();
            var books = bookIds.Any()
                ? await _db.Books.Where(b => bookIds.Contains(b.Id)).ToListAsync()
                : new List<Book>();

            var author = new Author
            {
                Name = vm.Name,
                Books = books
            };

            _db.Authors.Add(author);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var author = await _db.Authors
                                  .Include(a => a.Books)
                                  .FirstOrDefaultAsync(a => a.Id == id.Value);

            if (author == null) return NotFound();

            var vm = author.ToViewModel();
            ViewData["Books"] = await _db.Books.AsNoTracking().ToListAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AuthorViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewData["Books"] = await _db.Books.AsNoTracking().ToListAsync();
                return View(vm);
            }

            var author = await _db.Authors
                                  .Include(a => a.Books)
                                  .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null) return NotFound();

            author.Name = vm.Name;

            author.Books.Clear();
            var bookIds = vm.BookIds ?? new List<Guid>();
            if (bookIds.Any())
            {
                var selectedBooks = await _db.Books.Where(b => bookIds.Contains(b.Id)).ToListAsync();
                foreach (var b in selectedBooks) author.Books.Add(b);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var author = await _db.Authors
                                  .Include(a => a.Books)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(a => a.Id == id.Value);

            if (author == null) return NotFound();

            return View(author.ToViewModel());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var author = await _db.Authors.FindAsync(id);
            if (author != null)
            {
                _db.Authors.Remove(author);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}