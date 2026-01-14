using SteamLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = default!;

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = default!;

        [Required]
        public Guid AccessId { get; set; }

        [ForeignKey(nameof(AccessId))]
        public Access Access { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        
        // Many-to-many with Games (users can own/favorite games)
        public virtual ICollection<UserGame> Games { get; set; } = new List<UserGame>();

        
        // One-to-many: Users can add multiple games
        public virtual ICollection<Game> AddedGames { get; set; } = new List<Game>();

        
        // One-to-many: Users can create multiple publishers
        public virtual ICollection<Publisher> CreatedPublishers { get; set; } = new List<Publisher>();

    }
}
