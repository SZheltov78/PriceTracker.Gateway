namespace PriceTracker.Gateway.Entities
{
    public class SimpleRegistrationUser
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Tariff { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
