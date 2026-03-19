namespace PriceTracker.Gateway.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Tariff { get; set; } // "Free", "Medium"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
