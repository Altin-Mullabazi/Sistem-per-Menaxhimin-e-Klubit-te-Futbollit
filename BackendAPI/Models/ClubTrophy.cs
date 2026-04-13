using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class ClubTrophy
    {
        [Key]
        [Column(Order = 1)]
        [ForeignKey("Trophy")]
        public int TrophyId { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("Club")]
        public int ClubId { get; set; }

        [Required]
        [Range(1800, 2100)]
        public int YearWon { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Trophy? Trophy { get; set; }
        public virtual Club? Club { get; set; }
    }
}
