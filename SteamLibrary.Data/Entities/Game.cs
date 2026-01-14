using SteamLibrary.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary.Data.Entities
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Genre { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int? AgeRating { get; set; }

        public bool IsMultiplayer { get; set; }

        // Foreign key to Publisher
        public int? PublisherId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(PublisherId))]
        public virtual Publisher Publisher { get; set; } = null!;

        // Many-to-many with Users (if users can own/favorite games)
        public virtual ICollection<UserGame> Users { get; set; } = new List<UserGame>();

        // Audit properties
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relationship with User (who added the game)
        public int? AddedByUserId { get; set; }
        public virtual User? AddedByUser { get; set; }
    }
}
