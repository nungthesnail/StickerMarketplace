using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Logic.Controllers;

public class ProjectManagementController(
    IControllerContext ctx, IControllerFactory controllerFactory, IUserStateService userStateService,
    IExtendedBotClient bot, IAssetProvider assetProvider, IProjectService projectService, IStringParser parser)
    : AbstractController<ProjectManagementState>(ctx, controllerFactory, userStateService)
{
    private const string ViewStatCallback = "viewProjectStat";
    private const string DeleteCallback = "deleteProject";
    
    public override async Task HandleUpdateAsync(CancellationToken stoppingToken = default)
    {
        if (Update.CallbackQuery?.Data?.StartsWith(ViewStatCallback) ?? false)
            await ViewStatAsync(stoppingToken);
        else if (Update.CallbackQuery?.Data?.StartsWith(DeleteCallback) ?? false)
            await DeleteAsync(stoppingToken);
    }

    private async Task ViewStatAsync(CancellationToken stoppingToken)
    {
        var data = Update.CallbackQuery?.Data;
        if (data is null)
            return;
        if (data == ViewStatCallback) // When there's no parameters selecting a project
            await SendProjectsAsync(ViewStatCallback, stoppingToken);
        else
            await ViewProjectStatAsync(data, stoppingToken);
    }

    private async Task SendProjectsAsync(string callbackMethod, CancellationToken stoppingToken)
    {
        var projects = await projectService.GetProjectsByUserIdAsync(UserState.UserId, stoppingToken);
        var replyMarkup = CreateProjectsMarkup(callbackMethod, projects);
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.ProjectManagementSelectProject,
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

    private InlineKeyboardMarkup CreateProjectsMarkup(string callbackMethod, IEnumerable<Project> projects)
    {
        var buttons = projects.Select(x => new []
            {
                new InlineKeyboardButton
                {
                    Text = x.Name,
                    CallbackData = $"{callbackMethod}_{x.Id}"
                }
            }).ToArray();
        
        return new InlineKeyboardMarkup
        {
            InlineKeyboard = buttons
        };
    }

    private async Task ViewProjectStatAsync(string data, CancellationToken stoppingToken)
    {
        var projectId = ExtractProjectId(data);
        if (projectId is null)
            return;
        var project = await projectService.GetProjectByIdAsync(projectId.Value, stoppingToken);
        if (project is null)
        {
            var noProjectReplica = assetProvider.GetTextReplica(AssetKeys.Text.ProjectManagementStat,
                out var parseMode1, out var imageUrl1, out var replyMarkup1, out var effectId1);
            await bot.SendAsync(
                userState: UserState,
                text: noProjectReplica,
                parseMode: parseMode1,
                photoUrl: imageUrl1,
                replyMarkup: replyMarkup1,
                messageEffectId: effectId1,
                stoppingToken: stoppingToken);
            return;
        }
        
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.ProjectManagementStat,
            out var parseMode, out _, out var replyMarkup, out var effectId,
            project.Name, project.CachedRating, project.Visible);
        await bot.SendAsync(
            userState: UserState,
            text: replica,
            parseMode: parseMode,
            fileId: project.ImageId,
            replyMarkup: replyMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
    }

    private long? ExtractProjectId(string callbackData)
    {
        const int paramIdx = 2;
        return long.TryParse(parser.ExtractParameter(callbackData, paramIdx), out var id) ? id : null;
    }
    
    private async Task DeleteAsync(CancellationToken stoppingToken)
    {
        var data = Update.CallbackQuery?.Data;
        if (data is null)
            return;
        if (data == ViewStatCallback) // When there's no parameters selecting a project
            await SendProjectsAsync(DeleteCallback, stoppingToken);
        else
            await DeleteProjectAsync(data, stoppingToken);
    }

    private async Task DeleteProjectAsync(string data, CancellationToken stoppingToken)
    {
        var projectId = ExtractProjectId(data);
        if (projectId is null)
            return;
        await projectService.DeleteProjectAsync(projectId.Value, stoppingToken);
        
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.ProjectManagementDeleted,
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

    public override async Task IntroduceAsync(CancellationToken stoppingToken = default)
    {
        var replyMarkup = CreateProjectActionsMarkup();
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.ProjectManagementSelectAction,
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

    private InlineKeyboardMarkup CreateProjectActionsMarkup()
    {
        var viewStatText = assetProvider.GetTextReplica(AssetKeys.Keyboards.ProjectViewStatistics);
        var deleteText = assetProvider.GetTextReplica(AssetKeys.Keyboards.ProjectDelete);

        return new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [
                    new InlineKeyboardButton
                    {
                        CallbackData = ViewStatCallback,
                        Text = viewStatText
                    }
                ],
                [
                    new InlineKeyboardButton
                    {
                        CallbackData = DeleteCallback,
                        Text = deleteText
                    }
                ]
            ]
        };
    }
}
