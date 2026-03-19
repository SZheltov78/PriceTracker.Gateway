using PriceTracker.Ozon.Worker.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Ozon.Worker.Services
{
    public interface IOzonParserApi
    {
        Task<OzonParserResponse?> GetPriceAsync(string url, CancellationToken cancellationToken = default);
    }
}
