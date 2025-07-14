using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Logic.Exceptions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;
using Marketplace.Core.Models.UserStates;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Bot.Logic.Controllers;

public class ProjectCreationController(
    IControllerContext ctx, IControllerFactory controllerFactory, IUserStateService userStateService,
    IExtendedBotClient bot, IAssetProvider assetProvider, IProjectCategoryService projectCategoryService,
    IProjectService projectService, IProjectTagService projectTagService, IKeyboardFactory keyboardFactory,
    ILogger<ProjectCreationController> logger)
    : AbstractController<ProjectCreationUserState>(ctx, controllerFactory, userStateService)
{
    public override async Task HandleUpdateAsync(CancellationToken stoppingToken = default)
    {
        switch (UserState.Progress)
        {
            case ProjectCreationProgress.SetCategory:
                await SetCategoryAsync(stoppingToken);
                break;
            case ProjectCreationProgress.SetName:
                await SetNameAsync(stoppingToken);
                break;
            case ProjectCreationProgress.SetDescription:
                await SetDescriptionAsync(stoppingToken);
                break;
            case ProjectCreationProgress.SetImageId:
                await UploadImageAsync(stoppingToken);
                break;
            case ProjectCreationProgress.SetContentUrl:
                await SetContentUrlAsync(stoppingToken);
                break;
            case ProjectCreationProgress.SetProjectTag:
                await SetProjectTagAsync(stoppingToken);
                break;
            case ProjectCreationProgress.Completed:
                await CompletedAsync(stoppingToken);
                break;
            default:
                throw new InvalidOperationException($"Invalid progress value: {UserState.Progress}");
        }
    }

    private async Task SetCategoryAsync(CancellationToken stoppingToken)
    {
        if (Update.CallbackQuery is null)
            return;
        if (!Enum.TryParse<CategoryIdentifier>(Update.CallbackQuery.Data, out var categoryId))
            throw new UnknownCategoryException(Update.CallbackQuery.Data);
        UserState.Category = categoryId;
        UserState.Progress = ProjectCreationProgress.SetName;
        await SendSetNameMessage(stoppingToken);
    }

    private async Task SendSetNameMessage(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationSetName,
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

    private async Task SetNameAsync(CancellationToken stoppingToken)
    {
        const int maxNameLength = 128;
        
        if (Update.Message?.Text is null)
            return;
        var name = Update.Message.Text.Trim();
        if (name.Length > maxNameLength)
        {
            await SendNameIsTooLongAsync(stoppingToken);
            return;
        }
        if (!await projectService.IsNameAvailableAsync(name, stoppingToken))
        {
            await SendNameIsNotAvailableAsync(stoppingToken);
            return;
        }

        UserState.ProjectName = name;
        UserState.Progress = ProjectCreationProgress.SetDescription;
        await SendSetDescriptionAsync(stoppingToken);
    }

    private async Task SendNameIsTooLongAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationNameIsTooLong,
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

    private async Task SendNameIsNotAvailableAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationNameIsNotAvailable,
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

    private async Task SendSetDescriptionAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationSetDescription,
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

    private async Task SetDescriptionAsync(CancellationToken stoppingToken)
    {
        const int maxDescriptionLength = 512;
        
        if (Update.Message?.Text is null)
            return;
        var name = Update.Message.Text.Trim();
        if (name.Length > maxDescriptionLength)
        {
            await SendDescriptionIsTooLongAsync(stoppingToken);
            return;
        }
        
        UserState.ProjectDescription = name;
        UserState.Progress = ProjectCreationProgress.SetImageId;
        await SendUploadImageAsync(stoppingToken);
    }

    private async Task SendDescriptionIsTooLongAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationDescriptionIsTooLong,
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

    private async Task SendUploadImageAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationUploadImage,
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

    private async Task UploadImageAsync(CancellationToken stoppingToken)
    {
        if (Update.Message?.Photo?.FileId is null)
            return;
        
        UserState.ProjectImageId = Update.Message.Photo.FileId;
        UserState.Progress = ProjectCreationProgress.SetContentUrl;
        await SendSetContentUrlAsync(stoppingToken);
    }

    private async Task SendSetContentUrlAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationSetContentUrl,
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

    private async Task SetContentUrlAsync(CancellationToken stoppingToken)
    {
        const int maxContentUrlLength = 1024;
        
        if (Update.Message?.Text is null)
            return;
        var url = Update.Message.Text.Trim();
        if (url.Length > maxContentUrlLength || !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
        {
            await SendInvalidUrlAsync(stoppingToken);
            return;
        }
        if (!await projectService.IsUrlUniqueAsync(url, stoppingToken))
        {
            await SendProjectAlreadyExistsAsync(stoppingToken);
            return;
        }

        UserState.ProjectContentUrl = url;
        UserState.Progress = ProjectCreationProgress.SetProjectTag;
        await SendSelectTagAsync(stoppingToken);
    }

    private async Task SendInvalidUrlAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationInvalidUrl,
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

    private async Task SendProjectAlreadyExistsAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationProjectAlreadyExists,
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

    private async Task SendSelectTagAsync(CancellationToken stoppingToken)
    {
        var tags = await projectTagService.GetTagsAsync(stoppingToken);
        var replyMarkup = keyboardFactory.CreateTagsKeyboard(tags.ToList());
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationSelectTag,
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
    
    private async Task SetProjectTagAsync(CancellationToken stoppingToken)
    {
        if (Update.CallbackQuery?.Data is null)
            return;
        if (!long.TryParse(Update.CallbackQuery.Data, out var tagId))
            throw new FormatException($"Can't parse tag id: {Update.CallbackQuery.Data}");
        UserState.ProjectTagId = tagId;
        
        await CreateProjectAsync(stoppingToken);
        UserState.Progress = ProjectCreationProgress.Completed;
        await SendCompletedAsync(stoppingToken);
    }

    private async Task CreateProjectAsync(CancellationToken stoppingToken)
    {
        if (UserState.ProjectTagId is null)
            throw new InvalidOperationException("Tag id is null");
        
        var project = new Project
        {
            UserId = UserState.UserId,
            CategoryId = UserState.Category,
            ContentUrl = UserState.ProjectContentUrl,
            Description = UserState.ProjectDescription,
            Name = UserState.ProjectName ?? throw new InvalidOperationException("Project name is null"),
            ImageId = UserState.ProjectImageId,
            TagId = UserState.ProjectTagId.Value
        };

        try
        {
            await projectService.CreateProjectAsync(project, stoppingToken);
            logger.LogDebug("Project created: {name}", project.Name);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Failed to create project");
            throw;
        }
    }

    private async Task SendCompletedAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationCompleted,
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

    private async Task CompletedAsync(CancellationToken stoppingToken)
    {
        await ChangeToNewStateAsync<DefaultUserState>(stoppingToken: stoppingToken);
    }

    public override async Task IntroduceAsync(CancellationToken stoppingToken = default)
    {
        var categories = await projectCategoryService.GetAllAsync(stoppingToken);
        var replyMarkup = CreateCategoriesMarkup(categories);
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.CreationSetCategory,
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
    
    private ReplyMarkup CreateCategoriesMarkup(IEnumerable<ProjectCategory> categories)
        => keyboardFactory.CreateCategoriesKeyboard(categories);
}
