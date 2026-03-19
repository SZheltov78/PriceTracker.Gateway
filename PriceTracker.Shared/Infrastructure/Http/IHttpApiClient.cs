using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Shared.Infrastructure.Http
{
    public interface IHttpApiClient
    {
        Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);

        Task<T?> PostAsync<T>(string requestUri, object data, CancellationToken cancellationToken = default);

        Task<T?> PostAsync<T>(string requestUri, HttpContent content, CancellationToken cancellationToken = default);

        void SetBearerToken(string token);
    }
}
