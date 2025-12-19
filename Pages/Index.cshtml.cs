using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WiseLabels.Models;
using System.Text.Json;
using CERM.DataAccess.Models;
using CERM.DataAccess.Repositories.Substrate;

namespace WiseLabels.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ISubstrateRepository _substrateRepo;

        public Substrates? Substrate { get; set; }

        public IndexModel(ILogger<IndexModel> logger, ISubstrateRepository substrateRepo)
        {
            _logger = logger;
            _substrateRepo = substrateRepo;
        }

        public QuoteRequest? SavedQuoteRequest { get; set; }

        public async Task OnGetAsync()
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
            
            try
            {
                Substrate = await _substrateRepo.GetSubstrateByIdAsync("000006");

                if (Substrate == null)
                {
                    _logger.LogWarning("Substrate with ID '000006' not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the substrate.");
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
                ColorCode = Request.Form["colorCode"].ToString(),
                Finish = Request.Form["finish"].ToString(),
                ApplicationMethod = Request.Form["applicationMethod"].ToString(),
                UnwindDirection = Request.Form["unwindDirection"].ToString(),
                TotalQuantity = Request.Form["totalQuantity"].ToString(),
                ArtworkOption = Request.Form["artworkOption"].ToString(),
                
                // Form values for restoration
                ShapeValue = Request.Form["shapeValue"].ToString(),
                CornersValue = Request.Form["cornersValue"].ToString(),
                MaterialValue = Request.Form["materialValue"].ToString(),
                ColorCodeValue = Request.Form["colorCodeValue"].ToString(),
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
