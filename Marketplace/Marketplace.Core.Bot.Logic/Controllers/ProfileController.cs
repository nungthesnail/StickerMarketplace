using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.Enums.UserStates;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Logic.Controllers;

public class ProfileController(
    IControllerContext ctx, IControllerFactory controllerFactory, IUserStateService userStateService,
    IExtendedBotClient bot, IAssetProvider assetProvider, IUserService userService)
    : AbstractController<MyProfileUserState>(ctx, controllerFactory, userStateService)
{
    public override async Task HandleUpdateAsync(CancellationToken stoppingToken = default)
    {
        const string editName = "editName";
        const string renewSubscription = "renewSubscription";
        
        switch (Update.CallbackQuery?.Data)
        {
            case editName:
                await EditNameAsync(stoppingToken);
                break;
            case renewSubscription:
                await RenewSubscriptionAsync(stoppingToken);
                break;
        }
    }

    private async Task EditNameAsync(CancellationToken stoppingToken)
    {
        var state = new EditUserDataState
        {
            UserId = UserState.UserId,
            LastMessageId = UserState.LastMessageId,
            AttributeToEdit = UserDataAttribute.Name
        };
        await ChangeToNewStateAsync(state, stoppingToken);
    }

    private async Task RenewSubscriptionAsync(CancellationToken stoppingToken)
    {
        await ChangeToNewStateAsync<SubscriptionActivationState>(stoppingToken: stoppingToken);
    }

    public override async Task IntroduceAsync(CancellationToken stoppingToken = default)
    {
        var subscriptionActiveUntil
            = User.Subscription?.EnhancedUntil > User.Subscription?.BaseActiveUntil
                ? User.Subscription?.EnhancedUntil
                : User.Subscription?.BaseActiveUntil;
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.ProfileInfo,
            out var parseMode, out var imageUrl, out var replyMarkup, out var effectId,
            User.Name, User.RegisteredAt, subscriptionActiveUntil);
        
        await bot.SendAsync(
            userState: UserState,
            text: replica,
            parseMode: parseMode,
            photoUrl: imageUrl,
            replyMarkup: replyMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
    }
}
