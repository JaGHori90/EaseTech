using Api.Enums;
using Api.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.Dtos
{
    public class InvoiceListeDto
    {
        public int InvoiceId { get; set; }

        public int OrderId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string ServiceName { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; } 

        public decimal TaxAmount { get; set; }

        public decimal NetAmount { get; set; } 

        public DateTime OrderDate { get; set; }

        public DateTime InvoiceDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentMethod PaymentMethod { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatus PaymentStatus { get; set; }

        public string PaymentReference { get; set; } = string.Empty;
    }
}
