using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamLibrary.Entities;

namespace SteamLibrary.Data.Entities
{
    internal class Game
    {
        public string Title { get; set; }

        public double Size { get; set; }

        public int DalyPlayes { get; set; }

        public User User { get; set; }
    }
}
