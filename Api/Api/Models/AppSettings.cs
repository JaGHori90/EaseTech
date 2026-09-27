namespace Api.Models
{
    public class AppSettings
    {
        public string JWT_Secret { get; set; }

        public int TokenLifetimeHours { get; set; } = 12;
    }
}
