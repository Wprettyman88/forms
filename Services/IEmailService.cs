namespace WiseLabels.Services
{
    public interface IEmailService
    {
        Task<bool> SendQuoteConfirmationAsync(string toEmail, string quoteId, string customerName);
    }
}

