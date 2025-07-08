namespace Marketplace.Core.Bot.Models;

public class LabeledPrice
{
    public required string Label { get; set; }
    public required int Amount { get; set; }
}
