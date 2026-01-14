using SteamLibrary.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary.Data.Entities
{
    public class Publisher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        public DateTime FoundedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Game> Games { get; set; } = new List<Game>();

        // Audit properties (optional but recommended)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relationship with User (if publishers are managed by users)
        public int? CreatedByUserId { get; set; }
        public virtual User? CreatedByUser { get; set; }
    }
}
