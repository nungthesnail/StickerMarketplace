using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Logic.Controllers;

public class WelcomeController(IControllerContext ctx, IAssetProvider assetProvider, IExtendedBotClient bot,
    IUserStateService userStateService, IControllerFactory controllerFactory)
    : AbstractController<DefaultUserState>(ctx, controllerFactory, userStateService)
{
    public override async Task IntroduceAsync(CancellationToken stoppingToken = default)
        => await SendWelcomeMessageAsync(stoppingToken);

    public override async Task HandleUpdateAsync(CancellationToken stoppingToken = default)
    {
        if (Update.Message is not null)
        {
            if (Update.Message.Text == assetProvider.GetTextReplica(AssetKeys.Text.WelcomeGoToCatalog))
            {
                await GoToCatalogAsync(stoppingToken);
                return;
            }
            
            if (Update.Message.Text == assetProvider.GetTextReplica(AssetKeys.Text.WelcomeCreateContent))
            {
                await GoToContentCreationAsync(stoppingToken);
                return;
            }

            if (Update.Message.Text == assetProvider.GetTextReplica(AssetKeys.Text.WelcomeMyProfile))
            {
                await OpenMyProfileAsync(stoppingToken);
                return;
            }
        }
        
        await IntroduceAsync(stoppingToken);
    }

    private async Task SendWelcomeMessageAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.Welcome,
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

    private async Task GoToCatalogAsync(CancellationToken stoppingToken)
        => await ChangeToNewStateAsync<ProjectSearchUserState>(stoppingToken: stoppingToken);

    private async Task GoToContentCreationAsync(CancellationToken stoppingToken)
        => await ChangeToNewStateAsync<ProjectCreationUserState>(stoppingToken: stoppingToken);

    private async Task OpenMyProfileAsync(CancellationToken stoppingToken)
        => await ChangeToNewStateAsync<MyProfileUserState>(stoppingToken: stoppingToken);
}
