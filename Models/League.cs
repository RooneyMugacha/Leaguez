using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace Leaguez.Models
{
    public class League
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }=null!;

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Player> Players { get; set; }
        public virtual ICollection<Match> Matches { get; set; }

        public League()
        {
            Players = new HashSet<Player>();
            Matches = new HashSet<Match>();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
