using Microsoft.Extensions.Options;
using PriceTracker.Shared.Constants;
using PriceTracker.Shared.Infrastructure.MessageBus;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Shared.DTO
{
    public class HistoryResponse : IHasCorrelationId
    {
        public string CorrelationId { get; set; }
        public List<ProductHistoryDto> Products { get; set; }        
        public HistoryResponse()
        {
            Products = new List<ProductHistoryDto>();
        }
    }

    public class ProductHistoryDto
    {
        public string ProductName { get; set; }
        public string Url { get; set; }
        public TStatus Status { get; set; }
        public List<PricePointDto> PriceHistory { get; set; }
        public ProductHistoryDto()
        {
            PriceHistory = new List<PricePointDto>();
        }
    }

    public class PricePointDto
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }
}
