using System.ComponentModel.DataAnnotations;

namespace BookGenerator.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public int PublicationYear { get; set; }

        public string? ThumbnailUrl { get; set; }

        public string? Description { get; set; }

        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }
    }
}
