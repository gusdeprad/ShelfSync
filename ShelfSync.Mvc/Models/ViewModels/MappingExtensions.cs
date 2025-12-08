using ShelfSync.Mvc.Models.Entities;

namespace ShelfSync.Mvc.Models.ViewModels
{
    public static class MappingExtensions
    {
        public static BookViewModel ToViewModel(this Book book)
        {
            return new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                AuthorIds = book.Authors?.Select(a => a.Id).ToList() ?? new List<Guid>(),
                Authors = book.Authors?.Select(a => a.ToViewModel()).ToList() ?? new List<AuthorViewModel>()
            };
        }

        public static AuthorViewModel ToViewModel(this Author author)
        {
            return new AuthorViewModel
            {
                Id = author.Id,
                Name = author.Name,
                BookIds = author.Books?.Select(b => b.Id).ToList() ?? new List<Guid>()
            };
        }

        public static Book ToEntity(this BookViewModel vm, IEnumerable<Author> availableAuthors)
        {
            var authors = availableAuthors.Where(a => vm.AuthorIds.Contains(a.Id)).ToList();
            return new Book
            {
                Id = vm.Id == Guid.Empty ? Guid.NewGuid() : vm.Id,
                Title = vm.Title,
                Authors = authors
            };
        }

        public static Author ToEntity(this AuthorViewModel vm, IEnumerable<Book> availableBooks)
        {
            var books = availableBooks.Where(b => vm.BookIds.Contains(b.Id)).ToList();
            return new Author
            {
                Id = vm.Id == Guid.Empty ? Guid.NewGuid() : vm.Id,
                Name = vm.Name,
                Books = books
            };
        }
    }
}