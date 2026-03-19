using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Ozon.Worker.DTO
{
    public class OzonParserResponse
    {
        public decimal Price { get; set; }
        public string Currency { get; set; } 
        public DateTime Date { get; set; }
    }
}
