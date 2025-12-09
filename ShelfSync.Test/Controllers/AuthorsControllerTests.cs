using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSync.Mvc.Controllers;
using ShelfSync.Mvc.Data;
using ShelfSync.Mvc.Models.Entities;
using ShelfSync.Mvc.Models.ViewModels;

namespace ShelfSync.Test.Controllers
{
    [TestClass]
    public class AuthorsControllerTests
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
        public async Task Index_Returns_Authors_ViewModel_List()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var ctx = CreateContext(dbName))
            {
                var book = new Book { Id = Guid.NewGuid(), Title = "B1" };
                var author = new Author { Id = Guid.NewGuid(), Name = "A1", Books = new List<Book> { book } };
                ctx.Books.Add(book);
                ctx.Authors.Add(author);
                await ctx.SaveChangesAsync();
            }

            using (var ctx = CreateContext(dbName))
            {
                var ctrl = new AuthorsController(ctx);
                var result = await ctrl.Index();

                Assert.IsInstanceOfType(result, typeof(ViewResult));
                var vr = (ViewResult)result;
                Assert.IsInstanceOfType(vr.Model, typeof(IEnumerable<AuthorViewModel>));
                var list = ((IEnumerable<AuthorViewModel>)vr.Model).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("A1", list[0].Name);
            }
        }

        #endregion

        #region Details

        [TestMethod]
        public async Task Details_NullId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new AuthorsController(ctx);

            var result = await ctrl.Details(null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_UnknownId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new AuthorsController(ctx);

            var result = await ctrl.Details(Guid.NewGuid());

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_Returns_ViewModel_For_Existing_Author()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid authorId;
            using (var ctx = CreateContext(dbName))
            {
                var a = new Author { Id = Guid.NewGuid(), Name = "Auth" };
                ctx.Authors.Add(a);
                await ctx.SaveChangesAsync();
                authorId = a.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new AuthorsController(ctx2);
            var result = await ctrl.Details(authorId);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsInstanceOfType(vr.Model, typeof(AuthorViewModel));
            var vm = (AuthorViewModel)vr.Model;
            Assert.AreEqual("Auth", vm.Name);
        }

        #endregion

        #region Create GET

        [TestMethod]
        public async Task Create_Get_Returns_View_And_Books_In_ViewData()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var ctx = CreateContext(dbName))
            {
                ctx.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Book1" });
                await ctx.SaveChangesAsync();
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new AuthorsController(ctx2);

            var result = await ctrl.Create();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var books = ctrl.ViewData["Books"] as List<Book>;
            Assert.IsNotNull(books);
            Assert.AreEqual(1, books.Count);
        }

        #endregion

        #region Create POST

        [TestMethod]
        public async Task Create_Post_InvalidModel_Returns_View_With_Books()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var ctx = CreateContext(dbName))
            {
                ctx.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Book1" });
                await ctx.SaveChangesAsync();
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new AuthorsController(ctx2);
            ctrl.ModelState.AddModelError("Name", "Required");

            var vm = new AuthorViewModel();
            var result = await ctrl.Create(vm);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var books = ctrl.ViewData["Books"] as List<Book>;
            Assert.IsNotNull(books);
        }

        [TestMethod]
        public async Task Create_Post_Valid_Creates_Author_And_Redirects()
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
            var ctrl = new AuthorsController(ctx2);

            var vm = new AuthorViewModel { Name = "NewAuthor", BookIds = new List<Guid> { bookId } };
            var result = await ctrl.Create(vm);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual(nameof(AuthorsController.Index), redirect.ActionName);

            using var verifyCtx = CreateContext(dbName);
            var created = verifyCtx.Authors.Include(a => a.Books).FirstOrDefault(a => a.Name == "NewAuthor");
            Assert.IsNotNull(created);
            Assert.AreEqual(1, created.Books.Count);
        }

        [TestMethod]
        public async Task Create_Post_ValidWithoutBookIds_Creates_Author_And_Redirects()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx2 = CreateContext(dbName);
            var ctrl = new AuthorsController(ctx2);

            var vm = new AuthorViewModel { Name = "NewAuthor", BookIds = new List<Guid>() };
            var result = await ctrl.Create(vm);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual(nameof(AuthorsController.Index), redirect.ActionName);

            using var verifyCtx = CreateContext(dbName);
            var created = verifyCtx.Authors.Include(a => a.Books).FirstOrDefault(a => a.Name == "NewAuthor");
            Assert.IsNotNull(created);
            Assert.AreEqual(0, created.Books.Count);
        }

        #endregion

        #region Edit GET

        [TestMethod]
        public async Task Edit_Get_NullId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new AuthorsController(ctx);

            var result = await ctrl.Edit(null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Get_UnknownId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new AuthorsController(ctx);

            var result = await ctrl.Edit(Guid.NewGuid());

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Get_Returns_ViewModel_And_Books()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid authorId, bookId;
            using (var ctx = CreateContext(dbName))
            {
                var b = new Book { Id = Guid.NewGuid(), Title = "Book1" };
                var a = new Author { Id = Guid.NewGuid(), Name = "Auth", Books = new List<Book> { b } };
                ctx.Books.Add(b);
                ctx.Authors.Add(a);
                await ctx.SaveChangesAsync();
                authorId = a.Id;
                bookId = b.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new AuthorsController(ctx2);
            var result = await ctrl.Edit(authorId);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsInstanceOfType(vr.Model, typeof(AuthorViewModel));
            var vm = (AuthorViewModel)vr.Model;
            Assert.AreEqual("Auth", vm.Name);
            var books = ctrl.ViewData["Books"] as List<Book>;
            Assert.IsNotNull(books);
            Assert.AreEqual(1, books.Count);
        }

        #endregion

        #region Edit POST

        [TestMethod]
        public async Task Edit_Post_IdMismatch_Returns_BadRequest()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new AuthorsController(ctx);

            var vm = new AuthorViewModel { Id = Guid.NewGuid(), Name = "X" };
            var result = await ctrl.Edit(Guid.NewGuid(), vm);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Edit_Post_InvalidModel_Returns_View_With_Books()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var ctx = CreateContext(dbName))
            {
                ctx.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Book1" });
                await ctx.SaveChangesAsync();
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new AuthorsController(ctx2);
            ctrl.ModelState.AddModelError("Name", "Required");

            var vm = new AuthorViewModel { Id = Guid.NewGuid(), Name = "" };
            var result = await ctrl.Edit(vm.Id, vm);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var books = ctrl.ViewData["Books"] as List<Book>;
            Assert.IsNotNull(books);
        }

        [TestMethod]
        public async Task Edit_Post_Author_NotFound_Returns_NotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);
            var ctrl = new AuthorsController(ctx);

            var id = Guid.NewGuid();
            var vm = new AuthorViewModel { Id = id, Name = "Name" };

            var result = await ctrl.Edit(id, vm);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Post_Valid_Updates_Author_And_Redirects()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid authorId, b1Id, b2Id;
            using (var ctx = CreateContext(dbName))
            {
                var b1 = new Book { Id = Guid.NewGuid(), Title = "B1" };
                var b2 = new Book { Id = Guid.NewGuid(), Title = "B2" };
                var a = new Author { Id = Guid.NewGuid(), Name = "OldName", Books = new List<Book> { b1 } };
                ctx.Books.AddRange(b1, b2);
                ctx.Authors.Add(a);
                await ctx.SaveChangesAsync();
                authorId = a.Id;
                b1Id = b1.Id;
                b2Id = b2.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new AuthorsController(ctx2);

            var vm = new AuthorViewModel
            {
                Id = authorId,
                Name = "NewName",
                BookIds = new List<Guid> { b2Id }
            };

            var result = await ctrl.Edit(authorId, vm);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual(nameof(AuthorsController.Index), redirect.ActionName);

            using var verify = CreateContext(dbName);
            var updated = verify.Authors.Include(a => a.Books).FirstOrDefault(a => a.Id == authorId);
            Assert.IsNotNull(updated);
            Assert.AreEqual("NewName", updated.Name);
            Assert.AreEqual(1, updated.Books.Count);
            Assert.IsTrue(updated.Books.Any(b => b.Id == b2Id));
        }

        #endregion

        #region Delete

        [TestMethod]
        public async Task Delete_NullId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new AuthorsController(ctx);

            var result = await ctrl.Delete(null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_UnknownId_Returns_NotFound()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var ctrl = new AuthorsController(ctx);

            var result = await ctrl.Delete(Guid.NewGuid());

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_Returns_ViewModel_For_Existing_Author()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid authorId;
            using (var ctx = CreateContext(dbName))
            {
                var b = new Book { Id = Guid.NewGuid(), Title = "B1" };
                var a = new Author { Id = Guid.NewGuid(), Name = "ToView", Books = new List<Book> { b } };
                ctx.Books.Add(b);
                ctx.Authors.Add(a);
                await ctx.SaveChangesAsync();
                authorId = a.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new AuthorsController(ctx2);
            var result = await ctrl.Delete(authorId);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsInstanceOfType(vr.Model, typeof(AuthorViewModel));
            var vm = (AuthorViewModel)vr.Model;
            Assert.AreEqual("ToView", vm.Name);
        }

        #endregion

        #region DeleteConfirmed

        [TestMethod]
        public async Task DeleteConfirmed_Removes_Author_If_Exists()
        {
            var dbName = Guid.NewGuid().ToString();
            Guid authorId;
            using (var ctx = CreateContext(dbName))
            {
                var a = new Author { Id = Guid.NewGuid(), Name = "ToDelete" };
                ctx.Authors.Add(a);
                await ctx.SaveChangesAsync();
                authorId = a.Id;
            }

            using var ctx2 = CreateContext(dbName);
            var ctrl = new AuthorsController(ctx2);

            var result = await ctrl.DeleteConfirmed(authorId);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            using var verify = CreateContext(dbName);
            var exists = verify.Authors.Find(authorId);
            Assert.IsNull(exists);
        }

        #endregion
    }
}