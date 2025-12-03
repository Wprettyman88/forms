using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WiseLabels.Models;
using System.Text.Json;

namespace WiseLabels.Pages
{
    public class SuccessModel : PageModel
    {
        public string? QuoteId { get; set; }
        public bool ApiSuccess { get; set; }
        public QuoteRequest? QuoteRequest { get; set; }
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        public void OnGet()
        {
            // Get quote ID and API success status from TempData
            QuoteId = TempData["QuoteId"]?.ToString();
            
            if (bool.TryParse(TempData["ApiSuccess"]?.ToString(), out var apiSuccess))
            {
                ApiSuccess = apiSuccess;
            }

            // Get quote request data for PDF generation
            if (TempData.TryGetValue("QuoteRequest", out var quoteData))
            {
                try
                {
                    QuoteRequest = JsonSerializer.Deserialize<QuoteRequest>(quoteData.ToString() ?? "{}");
                    if (QuoteRequest != null && QuoteRequest.CreatedDate != default)
                    {
                        SubmittedDate = QuoteRequest.CreatedDate;
                    }
                }
                catch
                {
                    QuoteRequest = null;
                }
            }
        }
    }
}

