using PriceTracker.Shared.Constants;
using System;

namespace PriceTracker.Ozon.Worker.Entities
{
    public class OzonTask
    {
        public Guid Id { get; set; }    
        public string Name { get; set; }
        public string Url { get; set; } = string.Empty;
        public decimal ThresholdPrice { get; set; }
        public DateTime LastParseDate { get; set; }
        public DateTime ParseToDate { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public TStatus Status { get; set; }

        public ICollection<PriceHistory> PriceHistories { get; set; }        
    }
}
