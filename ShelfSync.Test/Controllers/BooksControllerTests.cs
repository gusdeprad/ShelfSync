using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSync.Mvc.Controllers;
using ShelfSync.Mvc.Data;
using ShelfSync.Mvc.Models.Entities;
using ShelfSync.Mvc.Models.ViewModels;

namespace ShelfSync.Test.Controllers
{
    [TestClass]
    public class BooksControllerTests
    {
        #region helpers

        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        #endregion

        #region Index

        [TestMethod]
        public async Task Index_Returns_BookViewModel_List()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var ctx = CreateContext(dbName))
            {
                var b = new Book { Id = Guid.NewGuid(), Title = "T1" };
                ctx.Books.Add(b);
                await ctx.SaveChangesAsync();
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);
            var result = await ctrl.Index();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsInstanceOfType(vr.Model, typeof(IEnumerable<BookViewModel>));
            var list = ((IEnumerable<BookViewModel>)vr.Model).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("T1", list[0].Title);
        }

        #endregion

        #region Details

        [TestMethod]
        public async Task Details_NullId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new BooksController(ctx);

            var result = await ctrl.Details(null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_UnknownId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new BooksController(ctx);

            var result = await ctrl.Details(Guid.NewGuid());

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_Returns_ViewModel_For_Existing_Book()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid bookId;
            using (var ctx = CreateContext(dbName))
            {
                var b = new Book { Id = Guid.NewGuid(), Title = "Book1" };
                ctx.Books.Add(b);
                await ctx.SaveChangesAsync();
                bookId = b.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);
            var result = await ctrl.Details(bookId);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsInstanceOfType(vr.Model, typeof(BookViewModel));
            var vm = (BookViewModel)vr.Model;
            Assert.AreEqual("Book1", vm.Title);
        }

        #endregion

        #region Create GET

        [TestMethod]
        public async Task Create_Get_Returns_View_And_Authors_In_ViewData()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var ctx = CreateContext(dbName))
            {
                ctx.Authors.Add(new Author { Id = Guid.NewGuid(), Name = "A1" });
                await ctx.SaveChangesAsync();
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);

            var result = await ctrl.Create();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var authors = ctrl.ViewData["Authors"] as List<Author>;
            Assert.IsNotNull(authors);
            Assert.AreEqual(1, authors.Count);
        }

        #endregion

        #region Create POST

        [TestMethod]
        public async Task Create_Post_InvalidModel_Returns_View_With_Authors()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var ctx = CreateContext(dbName))
            {
                ctx.Authors.Add(new Author { Id = Guid.NewGuid(), Name = "Auth1" });
                await ctx.SaveChangesAsync();
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);
            ctrl.ModelState.AddModelError("Name", "Required");

            var vm = new BookViewModel();
            var result = await ctrl.Create(vm);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var books = ctrl.ViewData["Authors"] as List<Author>;
            Assert.IsNotNull(books);
        }

        [TestMethod]
        public async Task Create_Post_Valid_Creates_Book_And_Redirects()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid authorId;
            using (var ctx = CreateContext(dbName))
            {
                var a = new Author { Id = Guid.NewGuid(), Name = "Auth1" };
                ctx.Authors.Add(a);
                await ctx.SaveChangesAsync();
                authorId = a.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);

            var vm = new BookViewModel { Title = "NewBook", AuthorIds = new List<Guid> { authorId } };
            var result = await ctrl.Create(vm);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual(nameof(BooksController.Index), redirect.ActionName);

            using var verify = CreateContext(dbName);
            var created = verify.Books.Include(b => b.Authors).FirstOrDefault(b => b.Title == "NewBook");
            Assert.IsNotNull(created);
            Assert.AreEqual(1, created.Authors.Count);
        }

        [TestMethod]
        public async Task Create_Post_ValidWithoutBookIds_Creates_Book_And_Redirects()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);

            var vm = new BookViewModel { Title = "NewBook", AuthorIds = new List<Guid>() };
            var result = await ctrl.Create(vm);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual(nameof(BooksController.Index), redirect.ActionName);

            using var verifyCtx = CreateContext(dbName);
            var created = verifyCtx.Books.Include(b => b.Authors).FirstOrDefault(b => b.Title == "NewBook");
            Assert.IsNotNull(created);
            Assert.AreEqual(0, created.Authors.Count);
        }

        #endregion

        #region Edit GET

        [TestMethod]
        public async Task Edit_Get_NullId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new BooksController(ctx);

            var result = await ctrl.Edit(null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Get_UnknownId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new BooksController(ctx);

            var result = await ctrl.Edit(Guid.NewGuid());

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Get_Returns_ViewModel_And_Authors()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid bookId;
            using (var ctx = CreateContext(dbName))
            {
                var a = new Author { Id = Guid.NewGuid(), Name = "AuthA" };
                var b = new Book { Id = Guid.NewGuid(), Title = "BookX", Authors = new List<Author> { a } };
                ctx.Authors.Add(a);
                ctx.Books.Add(b);
                await ctx.SaveChangesAsync();
                bookId = b.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);
            var result = await ctrl.Edit(bookId);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsInstanceOfType(vr.Model, typeof(BookViewModel));
            var vm = (BookViewModel)vr.Model;
            Assert.AreEqual("BookX", vm.Title);
            var authors = ctrl.ViewData["Authors"] as List<Author>;
            Assert.IsNotNull(authors);
            Assert.AreEqual(1, authors.Count);
        }

        #endregion

        #region Edit POST

        [TestMethod]
        public async Task Edit_Post_IdMismatch_Returns_BadRequest()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new BooksController(ctx);

            var vm = new BookViewModel { Id = Guid.NewGuid(), Title = "X" };
            var result = await ctrl.Edit(Guid.NewGuid(), vm);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Edit_Post_InvalidModel_Returns_View_With_Authors()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var ctx = CreateContext(dbName))
            {
                ctx.Authors.Add(new Author { Id = Guid.NewGuid(), Name = "Auth1" });
                await ctx.SaveChangesAsync();
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);
            ctrl.ModelState.AddModelError("Title", "Required");

            var id = Guid.NewGuid();
            var vm = new BookViewModel { Id = id, Title = "" };
            var result = await ctrl.Edit(id, vm);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var authors = ctrl.ViewData["Authors"] as List<Author>;
            Assert.IsNotNull(authors);
        }

        [TestMethod]
        public async Task Edit_Post_Book_NotFound_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new BooksController(ctx);

            var id = Guid.NewGuid();
            var vm = new BookViewModel { Id = id, Title = "Name" };

            var result = await ctrl.Edit(id, vm);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Post_Valid_Updates_Book_And_Redirects()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid bookId, a1Id, a2Id;
            using (var ctx = CreateContext(dbName))
            {
                var a1 = new Author { Id = Guid.NewGuid(), Name = "A1" };
                var a2 = new Author { Id = Guid.NewGuid(), Name = "A2" };
                var b = new Book { Id = Guid.NewGuid(), Title = "OldTitle", Authors = new List<Author> { a1 } };
                ctx.Authors.AddRange(a1, a2);
                ctx.Books.Add(b);
                await ctx.SaveChangesAsync();
                bookId = b.Id;
                a1Id = a1.Id;
                a2Id = a2.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);

            var vm = new BookViewModel
            {
                Id = bookId,
                Title = "NewTitle",
                AuthorIds = new List<Guid> { a2Id }
            };

            var result = await ctrl.Edit(bookId, vm);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual(nameof(BooksController.Index), redirect.ActionName);

            using var verify = CreateContext(dbName);
            var updated = verify.Books.Include(b => b.Authors).FirstOrDefault(b => b.Id == bookId);
            Assert.IsNotNull(updated);
            Assert.AreEqual("NewTitle", updated.Title);
            Assert.AreEqual(1, updated.Authors.Count);
            Assert.IsTrue(updated.Authors.Any(a => a.Id == a2Id));
        }

        #endregion

        #region Delete

        [TestMethod]
        public async Task Delete_NullId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new BooksController(ctx);

            var result = await ctrl.Delete(null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_UnknownId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new BooksController(ctx);

            var result = await ctrl.Delete(Guid.NewGuid());

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_Returns_ViewModel_For_Existing_Book()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid bookId;
            using (var ctx = CreateContext(dbName))
            {
                var a = new Author { Id = Guid.NewGuid(), Name = "AuthY" };
                var b = new Book { Id = Guid.NewGuid(), Title = "ToView", Authors = new List<Author> { a } };
                ctx.Authors.Add(a);
                ctx.Books.Add(b);
                await ctx.SaveChangesAsync();
                bookId = b.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);
            var result = await ctrl.Delete(bookId);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsInstanceOfType(vr.Model, typeof(BookViewModel));
            var vm = (BookViewModel)vr.Model;
            Assert.AreEqual("ToView", vm.Title);
        }

        #endregion

        #region DeleteConfirmed

        [TestMethod]
        public async Task DeleteConfirmed_Removes_Book_If_Exists()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid bookId;
            using (var ctx = CreateContext(dbName))
            {
                var b = new Book { Id = Guid.NewGuid(), Title = "ToDelete" };
                ctx.Books.Add(b);
                await ctx.SaveChangesAsync();
                bookId = b.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new BooksController(ctx2);

            var result = await ctrl.DeleteConfirmed(bookId);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            using var verify = CreateContext(dbName);
            var exists = verify.Books.Find(bookId);
            Assert.IsNull(exists);
        }

        #endregion
    }
}