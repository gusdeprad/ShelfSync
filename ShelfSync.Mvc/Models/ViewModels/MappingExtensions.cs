using ShelfSync.Mvc.Models.Entities;

namespace ShelfSync.Mvc.Models.ViewModels
{
    public static class MappingExtensions
    {
        public static BookViewModel ToViewModel(this Book book) => book.ToViewModel(maxDepth: 1);

        public static AuthorViewModel ToViewModel(this Author author) => author.ToViewModel(maxDepth: 1);

        public static BookViewModel ToViewModel(this Book book, int maxDepth)
        {
            if (book is null) throw new ArgumentNullException(nameof(book));

            return new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                AuthorIds = book.Authors?.Select(a => a.Id).ToList() ?? new List<Guid>(),
                Authors = (maxDepth > 0)
                    ? book.Authors?.Select(a => a.ToViewModel(maxDepth - 1)).ToList() ?? new List<AuthorViewModel>()
                    : new List<AuthorViewModel>(),
            };
        }

        public static AuthorViewModel ToViewModel(this Author author, int maxDepth)
        {
            if (author is null) throw new ArgumentNullException(nameof(author));

            return new AuthorViewModel
            {
                Id = author.Id,
                Name = author.Name,
                BookIds = author.Books?.Select(b => b.Id).ToList() ?? new List<Guid>(),
                Books = (maxDepth > 0)
                    ? author.Books?.Select(b => b.ToViewModel(maxDepth - 1)).ToList() ?? new List<BookViewModel>()
                    : new List<BookViewModel>(),
            };
        }

        public static Book ToEntity(this BookViewModel vm, IEnumerable<Author> availableAuthors)
        {
            var authorIds = vm.AuthorIds ?? Enumerable.Empty<Guid>();
            var authors = availableAuthors.Where(a => authorIds.Contains(a.Id)).ToList();
            return new Book
            {
                Id = vm.Id == Guid.Empty ? Guid.NewGuid() : vm.Id,
                Title = vm.Title,
                Authors = authors,
            };
        }

        public static Author ToEntity(this AuthorViewModel vm, IEnumerable<Book> availableBooks)
        {
            var bookIds = vm.BookIds ?? Enumerable.Empty<Guid>();
            var books = availableBooks.Where(b => bookIds.Contains(b.Id)).ToList();
            return new Author
            {
                Id = vm.Id == Guid.Empty ? Guid.NewGuid() : vm.Id,
                Name = vm.Name,
                Books = books,
            };
        }
    }
}