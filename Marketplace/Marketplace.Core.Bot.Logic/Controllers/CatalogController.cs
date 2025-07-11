using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Logic.Controllers;

public class CatalogController(
    IControllerContext ctx, IControllerFactory controllerFactory, IUserStateService userStateService,
    IExtendedBotClient bot, ICatalogService catalogService, ILikeService likeService, IAssetProvider assetProvider)
    : AbstractController<ViewCatalogUserState>(ctx, controllerFactory, userStateService)
{
    private const string CallbackMoveNext = "moveNext";
    private const string CallbackMovePrevious = "movePrev";
    private const string CallbackLike = "like";
    private const string CallbackDislike = "dislike";
    private const string CallbackComplaint = "complaint";

    private const char RatingMark = ' ';
    
    public override async Task HandleUpdateAsync(CancellationToken stoppingToken = default)
    {
        switch (Update.CallbackQuery?.Data)
        {
            case CallbackMoveNext:
                await MoveCatalogNextAsync(stoppingToken);
                return;
            case CallbackMovePrevious:
                await MoveCatalogPrevAsync(stoppingToken);
                return;
            case CallbackLike:
                await LikeAsync(stoppingToken);
                return;
            case CallbackDislike:
                await DislikeAsync(stoppingToken);
                return;
            case CallbackComplaint:
                await ComplaintAsync(stoppingToken);
                return;
        }
    }

    private async Task MoveCatalogNextAsync(CancellationToken stoppingToken)
    {
        UserState.ProjectView.CurrentIndex++;
        await UpdateProjectAsync(stoppingToken);
        await ShowProjectAsync(stoppingToken);
    }

    private async Task ShowProjectAsync(CancellationToken stoppingToken)
    {
        if (UserState.ProjectView.Project is null)
            await UpdateProjectAsync(stoppingToken);
        if (UserState.ProjectView.Project is null)
        {
            await SendNoProjectsMessageAsync(stoppingToken);
            return;
        }

        var title = UserState.ProjectView.Project.Name;
        var description = UserState.ProjectView.Project.Description;
        var imageId = UserState.ProjectView.Project.ImageId;
        var text = assetProvider.GetTextReplica(AssetKeys.Text.CatalogProjectView, out var parseMode,
            title, description);
        var userLike = await likeService.GetProjectLikeByUserAsync(
            UserState.UserId, UserState.ProjectView.Project.Id, stoppingToken);
        var replyMarkup = CreateProjectReplyMarkup(userLike);
        
        await bot.SendAsync(
            userState: UserState,
            text: text,
            parseMode: parseMode,
            fileId: imageId.ToString(),
            replyMarkup: replyMarkup,
            stoppingToken: stoppingToken);
    }

    private async Task UpdateProjectAsync(CancellationToken stoppingToken)
    {
        var projectView = await catalogService.GetProjectByIndexAsync(
            UserState.ProjectView.CurrentIndex, UserState.Filter, stoppingToken);
        UserState.ProjectView = projectView;
    }

    private async Task SendNoProjectsMessageAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CatalogNoProjectsFound,
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

    private InlineKeyboardMarkup CreateProjectReplyMarkup(bool? userLiked)
    {
        /*
         * Layout:
         * DOWNLOAD
         * PREV NEXT
         * DISL LIKE
         * COMPLAINT
         */
        var downloadText = assetProvider.GetTextReplica(AssetKeys.Keyboards.CatalogDownload);
        var nextText = assetProvider.GetTextReplica(AssetKeys.Keyboards.CatalogMoveNext);
        var prevText = assetProvider.GetTextReplica(AssetKeys.Keyboards.CatalogMovePrev);
        var likeText = assetProvider.GetTextReplica(AssetKeys.Keyboards.CatalogLike);
        var dislikeText = assetProvider.GetTextReplica(AssetKeys.Keyboards.CatalogDislike);
        var complaintText = assetProvider.GetTextReplica(AssetKeys.Keyboards.CatalogComplaint);
        var url = UserState.ProjectView.Project?.ContentUrl;
        return new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [
                    new InlineKeyboardButton
                    {
                        Text = downloadText,
                        Url = url
                    }
                ],
                [
                    new InlineKeyboardButton
                    {
                        Text = prevText,
                        CallbackData = CallbackMovePrevious
                    },
                    new InlineKeyboardButton
                    {
                        Text = nextText,
                        CallbackData = CallbackMoveNext
                    }
                ],
                [
                    new InlineKeyboardButton
                    {
                        Text = userLiked == false ? $"{RatingMark} {likeText}" : likeText,
                        CallbackData = CallbackDislike
                    },
                    new InlineKeyboardButton
                    {
                        Text = userLiked == true ? $"{RatingMark} {dislikeText}" : dislikeText,
                        CallbackData = CallbackDislike
                    }
                ],
                [
                    new InlineKeyboardButton
                    {
                        Text = complaintText,
                        CallbackData = CallbackComplaint
                    }
                ]
            ]
        };
    }
    
    private async Task MoveCatalogPrevAsync(CancellationToken stoppingToken)
    {
        UserState.ProjectView.CurrentIndex++;
        await UpdateProjectAsync(stoppingToken);
        await ShowProjectAsync(stoppingToken);
    }

    private async Task LikeAsync(CancellationToken stoppingToken)
    {
        await SetLikeAsync(true, stoppingToken);
    }

    private async Task SetLikeAsync(bool like, CancellationToken stoppingToken)
    {
        if (UserState.ProjectView.Project is null || UserState.LastMessageId is null)
            return;
        
        var userLiked = await likeService.GetProjectLikeByUserAsync(
            UserState.UserId, UserState.ProjectView.Project.Id, stoppingToken);
        if (like == userLiked)
            return;
        await likeService.LikeProjectAsync(UserState.UserId, UserState.ProjectView.Project.Id, like, stoppingToken);
        
        var replyMarkup = CreateProjectReplyMarkup(userLiked);
        await bot.EditReplyMarkupAsync(
            chatId: UserState.UserId,
            messageId: UserState.LastMessageId.Value,
            replyMarkup: replyMarkup,
            stoppingToken: stoppingToken);
    }

    private async Task DislikeAsync(CancellationToken stoppingToken)
    {
        await SetLikeAsync(false, stoppingToken);
    }

    private async Task ComplaintAsync(CancellationToken stoppingToken)
    {
        var newState = new ComplaintCreationUserState
        {
            UserId = UserState.UserId,
            LastMessageId = UserState.LastMessageId,
            ProjectId = UserState.ProjectView.Project?.Id
        };
        await ChangeToNewStateAsync(newState, stoppingToken);
    }

    public override async Task IntroduceAsync(CancellationToken stoppingToken = default)
    {
        UserState.ProjectView.CurrentIndex = 0;
        await ShowProjectAsync(stoppingToken);
    }
}
