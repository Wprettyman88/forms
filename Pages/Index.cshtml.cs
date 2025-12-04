using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WiseLabels.Models;
using System.Text.Json;

namespace WiseLabels.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public QuoteRequest? SavedQuoteRequest { get; set; }

        public void OnGet()
        {
            // Check if we're returning from confirmation page (Edit button)
            if (TempData.TryGetValue("QuoteRequest", out var quoteData))
            {
                try
                {
                    SavedQuoteRequest = JsonSerializer.Deserialize<QuoteRequest>(quoteData.ToString() ?? "{}");
                    // Keep the data in case user navigates away and comes back
                    TempData.Keep("QuoteRequest");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing quote request from TempData");
                    SavedQuoteRequest = null;
                }
            }
        }

        public IActionResult OnPostSubmit()
        {
            // Collect form data from Request.Form
            var quoteRequest = new QuoteRequest
            {
                // Contact information
                Name = Request.Form["name"].ToString().Trim(),
                Email = Request.Form["email"].ToString().Trim(),
                Phone = Request.Form["phone"].ToString().Trim(),
                
                // Display values
                Description = Request.Form["description"].ToString(),
                Shape = Request.Form["shape"].ToString(),
                LabelWidth = Request.Form["labelWidth"].ToString(),
                LabelHeight = Request.Form["labelHeight"].ToString(),
                Diameter = Request.Form["diameter"].ToString(),
                Corners = Request.Form["corners"].ToString(),
                CuttingDie = Request.Form["cuttingDie"].ToString(),
                Printing = Request.Form["printing"].ToString(),
                Material = Request.Form["material"].ToString(),
                Finish = Request.Form["finish"].ToString(),
                ApplicationMethod = Request.Form["applicationMethod"].ToString(),
                UnwindDirection = Request.Form["unwindDirection"].ToString(),
                TotalQuantity = Request.Form["totalQuantity"].ToString(),
                ArtworkOption = Request.Form["artworkOption"].ToString(),
                
                // Form values for restoration
                ShapeValue = Request.Form["shapeValue"].ToString(),
                CornersValue = Request.Form["cornersValue"].ToString(),
                MaterialValue = Request.Form["materialValue"].ToString(),
                FinishValue = Request.Form["finishValue"].ToString(),
                ApplicationMethodValue = Request.Form["applicationMethodValue"].ToString(),
                UnwindDirectionValue = Request.Form["unwindDirectionValue"].ToString(),
                ArtworkOptionValue = Request.Form["artworkOptionValue"].ToString(),
                CuttingDieValue = Request.Form["cuttingDieValue"].ToString(),
                PrintingValue = Request.Form["printingValue"].ToString()
            };

            // Store quote data in TempData to pass to confirmation page
            TempData["QuoteRequest"] = JsonSerializer.Serialize(quoteRequest);
            
            return RedirectToPage("/Confirm");
        }
    }
}
