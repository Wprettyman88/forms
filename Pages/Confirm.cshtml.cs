using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WiseLabels.Models;
using System.Text.Json;

namespace WiseLabels.Pages
{
    public class ConfirmModel : PageModel
    {
        private readonly ILogger<ConfirmModel> _logger;
        private readonly WiseLabels.Services.IQuoteService _quoteService;

        public ConfirmModel(ILogger<ConfirmModel> logger, WiseLabels.Services.IQuoteService quoteService)
        {
            _logger = logger;
            _quoteService = quoteService;
        }

        [BindProperty]
        public QuoteRequest QuoteRequest { get; set; } = new();

        public void OnGet()
        {
            // Get quote data from TempData (passed from form submission)
            // Store it in the property for display, but also keep it in TempData for Edit redirect
            if (TempData.TryGetValue("QuoteRequest", out var quoteData))
            {
                try
                {
                    var quoteJson = quoteData.ToString() ?? "{}";
                    QuoteRequest = JsonSerializer.Deserialize<QuoteRequest>(quoteJson) ?? new QuoteRequest();
                    // Keep the original JSON in TempData so it's available for Edit redirect
                    TempData["QuoteRequest"] = quoteJson;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing quote request from TempData");
                    QuoteRequest = new QuoteRequest();
                }
            }
        }

        public async Task<IActionResult> OnPostConfirmAsync()
        {
            // Get quote data from TempData
            if (!TempData.TryGetValue("QuoteRequest", out var quoteData))
            {
                return RedirectToPage("/Index");
            }

            try
            {
                var quote = JsonSerializer.Deserialize<QuoteRequest>(quoteData.ToString() ?? "{}");
                if (quote == null)
                {
                    return RedirectToPage("/Index");
                }

                // Store in database
                var quoteId = await _quoteService.StoreQuoteAsync(quote);

                // Submit to CERM API
                var apiSuccess = await _quoteService.SubmitToCermApiAsync(quote);

                // Store quote ID in TempData for success page
                TempData["QuoteId"] = quoteId;
                TempData["ApiSuccess"] = apiSuccess.ToString();

                return RedirectToPage("/Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing quote confirmation");
                // Preserve quote data in case of error
                TempData.Keep("QuoteRequest");
                ModelState.AddModelError("", "An error occurred while processing your quote. Please try again.");
                return Page();
            }
        }

        public IActionResult OnPostEdit()
        {
            // When Edit button is clicked, retrieve quote data from TempData
            // (it was preserved in OnGet) and repopulate it for the redirect
            if (TempData.TryGetValue("QuoteRequest", out var quoteData))
            {
                // Repopulate TempData to ensure it survives the redirect to Index
                var quoteJson = quoteData.ToString() ?? "{}";
                TempData["QuoteRequest"] = quoteJson;
                _logger.LogInformation("Quote data preserved in TempData for edit redirect");
            }
            else
            {
                // Fallback: try to serialize from QuoteRequest property if it's populated
                if (QuoteRequest != null && !string.IsNullOrEmpty(QuoteRequest.Description))
                {
                    TempData["QuoteRequest"] = JsonSerializer.Serialize(QuoteRequest);
                    _logger.LogInformation("Quote data serialized from QuoteRequest property for edit redirect");
                }
                else
                {
                    _logger.LogWarning("No quote data found to preserve for edit - TempData is empty and QuoteRequest is null/empty");
                }
            }
            
            return RedirectToPage("/Index");
        }

    }
}

