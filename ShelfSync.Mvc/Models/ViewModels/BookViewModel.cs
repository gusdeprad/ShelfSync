using System.ComponentModel.DataAnnotations;

namespace ShelfSync.Mvc.Models.ViewModels
{
    public class BookViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        public List<Guid> AuthorIds { get; set; } = new();

        public List<AuthorViewModel> Authors { get; set; } = new();
    }
}