using WiseLabels.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WiseLabels.Services
{
    public interface IQuoteService
    {
        Task<string> StoreQuoteAsync(QuoteRequest quote);
        Task<bool> SubmitToCermApiAsync(QuoteRequest quote);
        CermApiPayload TransformToApiPayload(QuoteRequest quote);
    }

    public class QuoteService : IQuoteService
    {
        private readonly ILogger<QuoteService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public QuoteService(ILogger<QuoteService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> StoreQuoteAsync(QuoteRequest quote)
        {
            // TODO: Implement database storage
            // This is a stub - replace with actual database implementation using Entity Framework or similar
            
            _logger.LogInformation("Storing quote in database (stubbed)");
            _logger.LogDebug("Quote data: {QuoteData}", JsonSerializer.Serialize(quote));
            
            // Simulate async database operation
            await Task.Delay(100);
            
            // In real implementation, this would:
            // 1. Create DbContext
            // 2. Add quote to database
            // 3. Save changes
            // 4. Return the generated quote ID
            
            var quoteId = Guid.NewGuid().ToString();
            _logger.LogInformation("Quote stored with ID: {QuoteId}", quoteId);
            
            return quoteId;
        }

        public CermApiPayload TransformToApiPayload(QuoteRequest quote)
        {
            // Return the API payload structure with example/dummy values
            // TODO: Map form elements to populate the actual values
            
            _logger.LogDebug("Transforming quote to API payload. FinishValue: '{FinishValue}', Finish: '{Finish}'", 
                quote.FinishValue ?? "null", quote.Finish ?? "null");
            
            var payload = new CermApiPayload
            {
                CustomerId = _configuration.GetConnectionString("CustomerID") ?? string.Empty,
                ContactId = _configuration.GetConnectionString("ContactID") ?? string.Empty,
                WindingId = "10", // Sheeted
                Outline = quote.ShapeValue ?? quote.Shape ?? null, // Map from shape ID (use ShapeValue which contains the ID from API)
                DieSizeId = quote.CuttingDieValue ?? quote.CuttingDie ?? null, // Map from cuttingDieValue
                SubstrateId = quote.MaterialValue ?? quote.Material ?? null, // Map from materialValue
                Description = quote.Description,
                PackingProcedureId = "152",
                PackingPriority = "Diameter",
                PackingNumber = 500,
                NumberOfProducts = int.TryParse(quote.TotalQuantity, out var quantity) ? quantity : 1,
                Width = double.TryParse(quote.LabelWidth, out var width) ? width : null,
                Height = double.TryParse(quote.LabelHeight, out var height) ? height : null,
                Radius = double.TryParse(quote.Diameter, out var diameter) ? diameter / 2.0 : null,
                PressRuns = new List<PressRun>
                {
                    new PressRun
                    {
                        ColourCodeIdFront = quote.Printing ?? quote.PrintingValue ?? quote.ColorCodeValue ?? quote.ColorCode, // Use the ID value from printing field
                        FinishingTypes = (!string.IsNullOrWhiteSpace(quote.FinishValue) || !string.IsNullOrWhiteSpace(quote.Finish)) 
                            ? new List<string> { quote.FinishValue ?? quote.Finish ?? string.Empty } 
                            : new List<string>() // Map finish value to FinishingTypes
                    }
                },
                Quantities = !string.IsNullOrWhiteSpace(quote.TotalQuantity) && int.TryParse(quote.TotalQuantity, out var qty) 
                    ? new List<int> { qty } 
                    : new List<int>() // Map totalQuantity to Quantities array
            };

            return payload;
        }

        public async Task<bool> SubmitToCermApiAsync(QuoteRequest quote)
        {
            // TODO: Implement CERM API submission
            // This is a stub - replace with actual CERM API call
            
            _logger.LogInformation("Submitting quote to CERM API (stubbed)");
            _logger.LogDebug("Quote data: {QuoteData}", JsonSerializer.Serialize(quote));
            
            try
            {
                // Transform QuoteRequest to CERM API format
                var apiPayload = TransformToApiPayload(quote);
                
                // Serialize the payload for logging
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                var payloadJson = JsonSerializer.Serialize(apiPayload, jsonOptions);
                _logger.LogInformation("Transformed API payload: {Payload}", payloadJson);
                
                // In real implementation, this would:
                // 1. Get OAuth token (reuse authentication logic from Materials API)
                // 2. POST to appropriate CERM endpoint with apiPayload
                // 3. Handle response and return success/failure
                
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                // Simulate API call
                await Task.Delay(200);
                
                _logger.LogInformation("Quote submitted to CERM API successfully (stubbed)");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting quote to CERM API");
                return false;
            }
        }
    }
}

