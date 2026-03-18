using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PriceTracker.Shared.Infrastructure.Http
{
    public class HttpApiClient : IHttpApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public HttpApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(requestUri, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return default;

                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
            }
            catch (HttpRequestException)
            {
                return default;
            }
            catch (TaskCanceledException)
            {
                return default;
            }
        }

        public async Task<T?> PostAsync<T>(string requestUri, object data, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return default;

                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
            }
            catch (HttpRequestException)
            {
                return default;
            }
            catch (TaskCanceledException)
            {
                return default;
            }
        }

        public async Task<T?> PostAsync<T>(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return default;

                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
            }
            catch (HttpRequestException)
            {
                return default;
            }
            catch (TaskCanceledException)
            {
                return default;
            }
        }
    }
}
