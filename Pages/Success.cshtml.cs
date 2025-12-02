using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WiseLabels.Pages
{
    public class SuccessModel : PageModel
    {
        public string? QuoteId { get; set; }
        public bool ApiSuccess { get; set; }

        public void OnGet()
        {
            // Get quote ID and API success status from TempData
            QuoteId = TempData["QuoteId"]?.ToString();
            
            if (bool.TryParse(TempData["ApiSuccess"]?.ToString(), out var apiSuccess))
            {
                ApiSuccess = apiSuccess;
            }
        }
    }
}

