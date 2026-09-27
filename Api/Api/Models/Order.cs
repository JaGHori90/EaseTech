using Api.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public string EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public AppUser Employee { get; set; }

        public string CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public AppUser Customer { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public int ServiceId { get; set; }
        public Service Service { get; set; }
       
        public DateTime Date { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus OrderStatus { get; set; }

        [Required]
        public decimal TotalCost { get; set; }
        [Required]
        public decimal TimeOfService { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentMethod PaymentMethod { get; set; }

        public DateTime Appointment {  get; set; }

    }
}
