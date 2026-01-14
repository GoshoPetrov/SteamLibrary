using SteamLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary.Data.Entities
{
    public class UserGame
    {
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int GameId { get; set; }
        public virtual Game Game { get; set; } = null!;

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        public bool IsFavorite { get; set; }
        public decimal? PurchasePrice { get; set; }
    }
}
