using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public class CallStream
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string CallId { get; set; } = string.Empty;
        [Required]
        public bool IsAvailable { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
