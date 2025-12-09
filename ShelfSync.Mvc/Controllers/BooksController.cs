using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSync.Mvc.Data;
using ShelfSync.Mvc.Models.Entities;
using ShelfSync.Mvc.Models.ViewModels;

namespace ShelfSync.Mvc.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BooksController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _db.Books
                                 .Include(b => b.Authors)
                                 .AsNoTracking()
                                 .ToListAsync();

            var vms = books.Select(b => b.ToViewModel());
            return View(vms);
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var book = await _db.Books
                                .Include(b => b.Authors)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(b => b.Id == id.Value);

            if (book == null) return NotFound();

            return View(book.ToViewModel());
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Authors"] = await _db.Authors.AsNoTracking().ToListAsync();
            return View(new BookViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Authors"] = await _db.Authors.AsNoTracking().ToListAsync();
                return View(vm);
            }

            var authorIds = vm.AuthorIds;
            var authors = authorIds.Any()
                ? await _db.Authors.Where(a => authorIds.Contains(a.Id)).ToListAsync()
                : new List<Author>();

            var book = new Book
            {
                Title = vm.Title,
                Authors = authors
            };

            _db.Books.Add(book);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var book = await _db.Books
                                .Include(b => b.Authors)
                                .FirstOrDefaultAsync(b => b.Id == id.Value);

            if (book == null) return NotFound();

            var vm = book.ToViewModel();
            ViewData["Authors"] = await _db.Authors.AsNoTracking().ToListAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BookViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewData["Authors"] = await _db.Authors.AsNoTracking().ToListAsync();
                return View(vm);
            }

            var book = await _db.Books
                                .Include(b => b.Authors)
                                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            book.Title = vm.Title;

            book.Authors.Clear();
            var authorIds = vm.AuthorIds;
            if (authorIds.Any())
            {
                var selectedAuthors = await _db.Authors.Where(a => authorIds.Contains(a.Id)).ToListAsync();
                foreach (var a in selectedAuthors)
                {
                    book.Authors.Add(a);
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var book = await _db.Books
                                .Include(b => b.Authors)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(b => b.Id == id.Value);

            if (book == null) return NotFound();

            return View(book.ToViewModel());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book != null)
            {
                _db.Books.Remove(book);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}