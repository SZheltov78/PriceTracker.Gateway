using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Shared.DTO
{
    public class HistoryRequest
    {
        public string Email { get; set; }
        public int Take { get; set; }
    }
}
