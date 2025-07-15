namespace Marketplace.Bot.Models;

public class PreCheckoutQuery
{
    public string Id { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public int TotalAmount { get; set; }
    public string InvoicePayload { get; set; } = null!;
}
