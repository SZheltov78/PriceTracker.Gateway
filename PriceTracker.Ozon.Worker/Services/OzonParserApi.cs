using PriceTracker.Ozon.Worker.DTO;
using PriceTracker.Shared.Infrastructure.Http;


namespace PriceTracker.Ozon.Worker.Services
{
    public class OzonParserApi : IOzonParserApi
    {
        private readonly IHttpApiClient _httpApiClient;

        private readonly string _ozonApiParsePriceEndpoint = "http://opars3245.cmpl.ru/api/v1/price";

        public OzonParserApi(IHttpApiClient httpApiClient)
        {
            _httpApiClient = httpApiClient;
        }

        public async Task<OzonParserResponse?> GetPriceAsync(string url, CancellationToken cancellationToken = default)
        {            
            var requesData = new
            {
                productUrl = url,
                ignoreDiscount = true,
                persistence = 12
            };

            var token = Environment.GetEnvironmentVariable("OZON_PARSER_API_TOKEN");
            _httpApiClient.SetBearerToken(token);

            var response = await _httpApiClient.PostAsync<OzonParserResponse>(_ozonApiParsePriceEndpoint, requesData, cancellationToken);
            if (response == null)
            {
                //logger
            }
            return response;
        }
    }
}
