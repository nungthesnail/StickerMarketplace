using Marketplace.Bot.Models;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Models;

namespace Marketplace.Core.Bot.Logic.Implementations;

public class KeyboardFactory(IAssetProvider assetProvider) : IKeyboardFactory
{
    public ReplyMarkup CreateCategoriesKeyboard(IEnumerable<ProjectCategory> categories)
    {
        return new InlineKeyboardMarkup
        {
            InlineKeyboard = categories.Select(x =>
                new List<InlineKeyboardButton>
                {
                    new()
                    {
                        Text = x.Name,
                        CallbackData = x.Id.ToString()
                    }
                })
        };
    }

    public ReplyMarkup CreateTagsKeyboard(List<ProjectTag> tags, List<long>? selected = null,
        bool createNextButton = false, string? finishCallback = null)
    {
        const char selectedIcon = '\u2705';
        
        selected ??= [];
        var keyboard = new List<List<InlineKeyboardButton>>();
        for (var i = 0; i < tags.Count; i += tags.Count - i == 2 ? 1 : 2)
        {
            var row = tags.Skip(i).Take(2);
            var buttons = row.Select(x => new InlineKeyboardButton
            {
                Text = selected.Contains(x.Id) ? $"{selectedIcon} {x.Name}" : x.Name,
                CallbackData = x.Id.ToString()
            }).ToList();
            keyboard.Add(buttons);
        }

        if (createNextButton)
        {
            keyboard.Add([
                new InlineKeyboardButton
                {
                    Text = assetProvider.GetTextReplica(AssetKeys.Keyboards.CatalogFilterTagsAreSelected),
                    CallbackData = finishCallback
                }
            ]);
        }

        return new InlineKeyboardMarkup
        {
            InlineKeyboard = keyboard
        };
    }
}
