using Marketplace.Core.Abstractions.Services;
using Marketplace.Bot.Abstractions;
using Marketplace.Bot.Models;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.Enums.UserStates;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Logic.Controllers;

public class EditUserDataController(
    IControllerContext ctx, IControllerFactory controllerFactory, IUserStateService userStateService,
    IExtendedBotClient bot, IAssetProvider assetProvider, IUserService userService)
    : AbstractController<EditUserDataState>(ctx, controllerFactory, userStateService)
{
    private static readonly Dictionary<UserDataAttribute, string> AttributeToAssetKey = new()
    {
        [UserDataAttribute.Name] = AssetKeys.Text.EditUserName
    };
    
    public override async Task HandleUpdateAsync(CancellationToken stoppingToken = default)
    {
        switch (UserState.AttributeToEdit)
        {
            case UserDataAttribute.Name:
                await SetUserNameAsync(stoppingToken);
                break;
            default:
                throw new InvalidOperationException($"Invalid user data attribute: {UserState.AttributeToEdit}");
        }
    }

    private async Task SetUserNameAsync(CancellationToken stoppingToken)
    {
        const int maxNameLength = 32;
        
        if (Update.Message?.Text is null)
            return;
        var name = Update.Message.Text.Trim();
        if (name.Length > maxNameLength)
        {
            await SendNameIsTooLongAsync(stoppingToken);
            return;
        }
        
        var success = await userService.ChangeUserNameAsync(UserState.UserId, name, stoppingToken);
        if (!success)
        {
            await SendNameIsNotAvailableAsync(stoppingToken);
            return;
        }
        
        await ChangeToNewStateAsync<DefaultUserState>(stoppingToken: stoppingToken);
    }

    private async Task SendNameIsTooLongAsync(CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.EditUserNameIsTooLong,
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
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.UserNameIsNotAvailable,
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
        var replicaKey = AttributeToAssetKey[UserState.AttributeToEdit];
        var replica = assetProvider.GetTextReplica(replicaKey,
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
}
