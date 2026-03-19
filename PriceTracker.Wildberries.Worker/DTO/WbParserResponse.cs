using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Wildberries.Worker.DTO
{
    public class WbParserResponse
    {
        public decimal Сost { get; set; }
        public decimal? Discount { get; set; }
        public DateTime? Date { get; set; }
    }
}
