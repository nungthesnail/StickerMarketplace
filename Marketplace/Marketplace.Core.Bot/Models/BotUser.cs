namespace Marketplace.Core.Bot.Models;

public class BotUser
{
    public long Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? Username { get; set; }
}
