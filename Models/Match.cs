using System.ComponentModel.DataAnnotations;

namespace Leaguez.Models
{
    public class Match
    {

        public int Id { get; set; }

        public int Player1Id { get; set; }
        public virtual Player? Player1 { get; set; }

        public int Player2Id { get; set; }
        public virtual Player? Player2 { get; set; }

        public int? Score1 { get; set; }
        public int? Score2 { get; set; }

        public bool IsPlayed { get; set; }

        // Foreign Key for League
        public int LeagueId { get; set; }
        public virtual League? League { get; set; }
    }
}
