namespace Marketplace.Bot.Models;

public class Update
{
    public int Id { get; set; }
    public Message? Message { get; set; }
    public CallbackQuery? CallbackQuery { get; set; }
    public PreCheckoutQuery? PreCheckoutQuery { get; set; }
}
