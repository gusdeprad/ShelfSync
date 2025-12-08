using System.ComponentModel.DataAnnotations;

namespace ShelfSync.Mvc.Models.Entities
{
    public class Book
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        public ICollection<Author> Authors { get; set; } = new List<Author>();
    }
}