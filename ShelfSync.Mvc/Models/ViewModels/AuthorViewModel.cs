using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShelfSync.Mvc.Models.ViewModels
{
    public class AuthorViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        public List<Guid> BookIds { get; set; } = new();
    }
}