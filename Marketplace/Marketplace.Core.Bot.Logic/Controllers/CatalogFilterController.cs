using Marketplace.Core.Abstractions.Services;
using Marketplace.Bot.Models;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Logic.Controllers;

public class CatalogFilterController(
    IControllerContext ctx, IControllerFactory controllerFactory, IUserStateService userStateService,
    IExtendedBotClient bot, IAssetProvider assetProvider, IProjectCategoryService projectCategoryService,
    IProjectTagService projectTagService, IKeyboardFactory keyboardFactory)
    : AbstractController<ProjectSearchUserState>(ctx, controllerFactory, userStateService)
{
    private const string AllTagsSelectedCallback = "allTagsSelected";
    
    public override async Task HandleUpdateAsync(CancellationToken stoppingToken = default)
    {
        switch (UserState.Progress)
        {
            case ProjectSearchProgress.Introduce:
                await IntroduceAsync(stoppingToken);
                break;
            case ProjectSearchProgress.SelectCategory:
                await SelectCategoryAsync(stoppingToken);
                break;
            case ProjectSearchProgress.SelectTags:
                await SelectTagsAsync(stoppingToken);
                break;
            case ProjectSearchProgress.Completed:
                await GoToCatalogAsync(stoppingToken);
                break;
            default:
                throw new InvalidOperationException($"Invalid catalog filter state: {UserState}");
        }
    }

    public override async Task IntroduceAsync(CancellationToken stoppingToken = default)
    {
        var categories = await projectCategoryService.GetAllAsync(stoppingToken);
        var replyMarkup = CreateCategoriesKeyboard(categories);
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CatalogFilterSelectCategory,
            out var parseMode, out var imageUrl, out _, out var effectId);
        await bot.SendAsync(
            userState: UserState,
            text: replica,
            parseMode: parseMode,
            photoUrl: imageUrl,
            replyMarkup: replyMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
    }

    private ReplyMarkup CreateCategoriesKeyboard(IEnumerable<ProjectCategory> categories)
        => keyboardFactory.CreateCategoriesKeyboard(categories);

    private async Task SelectCategoryAsync(CancellationToken stoppingToken)
    {
        if (Update.CallbackQuery is null)
        {
            await SendAwaitingCategoryMessageAsync(stoppingToken);
            return;
        }
        if (!Enum.TryParse<CategoryIdentifier>(Update.CallbackQuery.Data, out var categoryId))
        {
            await SendCallbackQueryParsingErrorMessageAsync(stoppingToken);
            return;
        }
        
        UserState.Filter.Category = categoryId;
        UserState.Progress = ProjectSearchProgress.SelectTags;
        var tags = await projectTagService.GetTagsAsync(stoppingToken);
        await SendSelectTagsMessageAsync(tags, stoppingToken);
    }

    private async Task SendAwaitingCategoryMessageAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CatalogFilterAwaitingCategory,
            out var parseMode, out var imageUrl, out var replyMarkup, out var effectId);
        await bot.SendAsync(
            userState: UserState,
            text: replica,
            parseMode: parseMode,
            photoUrl: imageUrl,
            replyMarkup: replyMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
    }

    private async Task SendCallbackQueryParsingErrorMessageAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CallbackQueryParseError,
            out var parseMode, out var imageUrl, out var replyMarkup, out var effectId);
        await bot.SendAsync(
            userState: UserState,
            text: replica,
            parseMode: parseMode,
            photoUrl: imageUrl,
            replyMarkup: replyMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
    }

    private async Task SendSelectTagsMessageAsync(IEnumerable<ProjectTag> tags, CancellationToken stoppingToken)
    {
        var replyMarkup = CreateTagsKeyboard(tags.ToList(), UserState.Filter.TagIds);
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CatalogFilterSelectTags,
            out var parseMode, out var imageUrl, out _, out var effectId);
        await bot.SendAsync(
            userState: UserState,
            text: replica,
            parseMode: parseMode,
            photoUrl: imageUrl,
            replyMarkup: replyMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
    }

    private ReplyMarkup CreateTagsKeyboard(List<ProjectTag> tags, List<long>? selected = null,
        bool createNextButton = false, string? callback = null)
        => keyboardFactory.CreateTagsKeyboard(tags, selected, createNextButton, callback);

    private async Task SelectTagsAsync(CancellationToken stoppingToken)
    {
        const string commandNotModerated = AssetKeys.Commands.CatalogFilterNotModerated;
        if (Update.Message?.Text == commandNotModerated && User.IsAdmin)
        {
            UserState.Filter.NotModerated = true;
            return;
        }
        if (Update.CallbackQuery is null)
        {
            await SendAwaitingCategoryMessageAsync(stoppingToken);
            return;
        }
        if (Update.CallbackQuery.Data == AllTagsSelectedCallback)
        {
            await GoToCatalogAsync(stoppingToken);
            return;
        }
        if (!long.TryParse(Update.CallbackQuery.Data, out var categoryId))
        {
            await SendCallbackQueryParsingErrorMessageAsync(stoppingToken);
            return;
        }

        if (!UserState.Filter.TagIds.Remove(categoryId))
            UserState.Filter.TagIds.Add(categoryId);
        var tags = await projectTagService.GetTagsAsync(stoppingToken);
        await EditSelectTagsMarkupAsync(tags, stoppingToken);
    }

    private async Task EditSelectTagsMarkupAsync(IEnumerable<ProjectTag> tags, CancellationToken stoppingToken)
    {
        var replyMarkup = CreateTagsKeyboard(tags.ToList(), UserState.Filter.TagIds);
        await bot.EditReplyMarkupAsync(
            chatId: UserState.UserId,
            messageId: UserState.LastMessageId!.Value,
            replyMarkup: replyMarkup,
            stoppingToken: stoppingToken);
    }

    private async Task GoToCatalogAsync(CancellationToken stoppingToken)
    {
        var newState = new ViewCatalogUserState
        {
            UserId = UserState.UserId,
            LastMessageId = UserState.LastMessageId,
            Filter = UserState.Filter
        };
        await ChangeToNewStateAsync(newState, stoppingToken);
    }
}
