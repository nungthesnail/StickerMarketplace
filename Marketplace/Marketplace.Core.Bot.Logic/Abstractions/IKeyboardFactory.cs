using Marketplace.Bot.Models;
using Marketplace.Core.Models;

namespace Marketplace.Core.Bot.Logic.Abstractions;

public interface IKeyboardFactory
{
    ReplyMarkup CreateCategoriesKeyboard(IEnumerable<ProjectCategory> categories);
    ReplyMarkup CreateTagsKeyboard(List<ProjectTag> tags, List<long>? selected = null,
        bool createNextButton = false, string? finishCallback = null);
}
