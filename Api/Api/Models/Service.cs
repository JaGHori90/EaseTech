using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        public string ServiceName { get; set; } = string.Empty;
        
        [Required]
        public decimal PricePerMinute { get; set; }

        [Required]
        public string ServiceDetails { get; set; } = string.Empty;
    }
}
