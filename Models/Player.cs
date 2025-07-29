using System.ComponentModel.DataAnnotations;

namespace Leaguez.Models
{
    public class Player
    {

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }=null!;

        public int Played { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }

        // Foreign Key for League
        public int LeagueId { get; set; }
        public virtual League? League { get; set; }
    }
}
