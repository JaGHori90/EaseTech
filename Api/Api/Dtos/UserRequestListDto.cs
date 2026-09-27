using Api.Enums;
using Api.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.Dtos
{
    public class UserRequestListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Referrer { get; set; } = string.Empty;
       
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContactStatus ContactStatus { get; set; }
    }
}
