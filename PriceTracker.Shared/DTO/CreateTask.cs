using PriceTracker.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Shared.DTO
{
    public class CreateTask
    {
        public string Email { get; set; }
        public string ProductName { get; set; }
        public string Url { get; set; }
        public decimal ThresholdPrice { get; set; }      
    }
}
