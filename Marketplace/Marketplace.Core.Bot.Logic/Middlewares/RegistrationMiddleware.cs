using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums.UserStates;
using Marketplace.Core.Models.UserStates;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Bot.Logic.Middlewares;

public partial class RegistrationMiddleware(IUserStateService userStateService, IAssetProvider assetProvider,
    IExtendedBotClient bot, IUserService userService, IPromocodeService promocodeService,
    IReferralInvitationService referralService, ILogger<RegistrationMiddleware> logger) : AbstractMiddleware
{
    public override async Task InvokeAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken = default)
    {
        if (update.Message is not null)
        {
            var message = update.Message;
            user = await userService.GetUserByIdAsync(
                userId: message.Chat.Id,
                includeSubscription: true,
                stoppingToken: stoppingToken);
            
            userState = userStateService.GetUserState(message.Chat.Id);
            if (user is null && (userState is null || userState is UserCreationState))
            {
                await ProcessUserCreationAsync((UserCreationState?)userState, update, stoppingToken);
                return;
            }

            if (IsStartMessage(update))
            {
                await SendHelloMessageAsync(update.Message.Chat.Id, stoppingToken);
                return;
            }
        }

        await (Next?.InvokeAsync(user, userState, update, stoppingToken) ?? Task.CompletedTask);
    }

    private async Task ProcessUserCreationAsync(UserCreationState? userState, Update update,
        CancellationToken stoppingToken)
    {
        if (userState is null)
            await InitializeUserCreationAsync(update, stoppingToken);
        else switch (userState.Progress)
        {
            case UserCreationProgress.SetName:
                await SetUserNameAsync(userState, update, stoppingToken);
                break;
            case UserCreationProgress.Completed:
                await CompleteUserCreationAsync(userState, update, stoppingToken);
                logger.LogInformation("User created: {userId}", userState.UserId);
                break;
            default:
                userState.Reset();
                throw new ArgumentOutOfRangeException(nameof(userState.Progress), userState.Progress, null);
        }
    }

    private bool IsStartMessage(Update update) =>
        update.Message?.Text?.StartsWith(assetProvider.GetTextReplica(AssetKeys.Commands.Start)) ?? false;

    private async Task InitializeUserCreationAsync(Update update, CancellationToken stoppingToken)
    {
        if (update.Message?.Text is null)
            return;
        
        string? promoCode = null;
        long? invitingUserId = null;
        if (IsStartMessage(update))
        {
            var messageTokens = GetMessageTokens(update.Message.Text);

            if (messageTokens.Length > 1)
            {
                var arguments = GetMessageArguments(messageTokens[1]);
                promoCode = ExtractPromoCode(arguments);
                invitingUserId = ExtractInvitingUserId(arguments);
            }
        }

        var userId = update.Message!.Chat.Id;
        var userState = new UserCreationState
        {
            UserId = userId,
            PromoCode = promoCode,
            InvitedByUserId = invitingUserId,
            LastMessageId = null,
            Progress = UserCreationProgress.SetName
        };
        
        userStateService.SetUserState(userId, userState);
        await SendHelloMessageAsync(userState, stoppingToken);
        await SendSetNameMessageAsync(userState, stoppingToken);

        return;
        
        static string[] GetMessageTokens(string text) => text.Split(' ');
        static string[] GetMessageArguments(string text) => text.Split('_');
    }

    private static string? ExtractPromoCode(IEnumerable<string> arguments)
    {
        const string argumentName = "promocode-";
        return arguments
            .Where(x => x.StartsWith(argumentName, StringComparison.Ordinal))
            .Select(x => x.Split("-")[1])
            .FirstOrDefault();
    }

    private static long? ExtractInvitingUserId(IEnumerable<string> arguments)
    {
        const string argumentName = "invitation-";
        var value = arguments
            .Where(x => x.StartsWith(argumentName, StringComparison.Ordinal))
            .Select(x => x.Split("-")[1])
            .FirstOrDefault();
        return value is null ? null : long.TryParse(value, out var userId) ? userId : null;
    }

    private async Task SendHelloMessageAsync(UserState userState, CancellationToken stoppingToken)
    {
        userState.LastMessageId = await SendHelloMessageAsync(userState.UserId, stoppingToken);
    }

    private async Task<int> SendHelloMessageAsync(long userId, CancellationToken stoppingToken)
    {
        return (await bot.SendMessageAsync(
            chatId: userId,
            text: assetProvider.GetTextReplica(AssetKeys.Text.Welcome, out var parseMode),
            parseMode: parseMode,
            stoppingToken: stoppingToken)).Id;
    }

    private async Task SendSetNameMessageAsync(UserState userState, CancellationToken stoppingToken = default)
    {
        userState.LastMessageId = (await bot.SendMessageAsync(
            chatId: userState.UserId,
            text: assetProvider.GetTextReplica(AssetKeys.Text.InputName, out var parseMode),
            parseMode: parseMode,
            stoppingToken: stoppingToken)).Id;
    }

    private async Task SetUserNameAsync(UserCreationState state, Update update, CancellationToken stoppingToken)
    {
        if (!ValidateUserName(update, out var userName))
        {
            await SendInvalidUserNameMessageAsync(state, stoppingToken);
            return;
        }
        var userNameAvailable = await IsUserNameAvailableAsync(userName, stoppingToken);
        if (!userNameAvailable)
        {
            await SendUserNameUnavailableMessageAsync(state, stoppingToken);
            return;
        }
        
        state.UserName = userName;
        state.MoveProgressNext();
        await ProcessUserCreationAsync(state, update, stoppingToken); // Because we don't wait input to create user
    }

    private static bool ValidateUserName(Update update, [NotNullWhen(true)] out string? userName)
    {
        const int minNameLength = 3;
        const int maxNameLength = 32;

        userName = null;
        if (update.Message is null || update.Message.Text is null)
            return false;
        
        userName = update.Message.Text;
        var regex = UserNameRegex();
        return regex.IsMatch(userName) && userName.Length is <= maxNameLength and >= minNameLength;
    }

    [GeneratedRegex("^[a-zA-Z0-9_]{3,32}$")]
    private static partial Regex UserNameRegex();

    private async Task SendInvalidUserNameMessageAsync(UserState userState, CancellationToken stoppingToken)
    {
        userState.LastMessageId = (await bot.SendMessageAsync(
            chatId: userState.UserId,
            text: assetProvider.GetTextReplica(AssetKeys.Text.InvalidUserName, out var parseMode),
            parseMode: parseMode,
            stoppingToken: stoppingToken)).Id;
    }

    private async Task<bool> IsUserNameAvailableAsync(string userName, CancellationToken stoppingToken)
    {
        return await userService.IsNameAvailableAsync(userName, stoppingToken);
    }
    
    private async Task SendUserNameUnavailableMessageAsync(UserState userState, CancellationToken stoppingToken)
    {
        userState.LastMessageId = (await bot.SendMessageAsync(
            chatId: userState.UserId,
            text: assetProvider.GetTextReplica(AssetKeys.Text.UserNameIsNotAvailable, out var parseMode),
            parseMode: parseMode,
            stoppingToken: stoppingToken)).Id;
    }

    private async Task CompleteUserCreationAsync(UserCreationState state, Update update,
        CancellationToken stoppingToken)
    {
        if (state.UserName is null)
            throw new InvalidOperationException("User name must be provided to create user");
        
        var userNameAvailable = await userService.IsNameAvailableAsync(state.UserName, stoppingToken);
        if (!userNameAvailable)
        {
            await SendUserNameUnavailableMessageAsync(state, stoppingToken);
            state.Reset();
            return;
        }

        var subscription = CreateSubscription(state.UserId);
        var user = CreateUser(state.UserId, state.UserName);
        await userService.CreateUserAsync(
            user: user,
            subscription: subscription,
            openTransaction: true,
            stoppingToken: stoppingToken);

        if (state.InvitedByUserId is not null)
        {
            var invitation = CreateReferralInvitation(state.UserId, state.InvitedByUserId.Value);
            await referralService.CreateInvitationAsync(
                invitation: invitation,
                openTransaction: true,
                updateInvitations: true,
                stoppingToken: stoppingToken);
        }

        if (state.PromoCode is not null)
            await ActivatePromocodeAsync(state.UserId, state.PromoCode, stoppingToken);
        
        var nextState = CreateNextState(state);
        userStateService.SetUserState(state.UserId, nextState);
        
        // To go to the next step
        await (Next?.InvokeAsync(user, nextState, update, stoppingToken) ?? Task.CompletedTask);
    }

    private Subscription CreateSubscription(long userId)
    {
        return new Subscription
        {
            UserId = userId
        };
    }

    private User CreateUser(long userId, string userName)
    {
        return new User
        {
            Id = userId,
            IsAdmin = false,
            Name = userName,
            RegisteredAt = DateTimeOffset.Now
        };
    }

    private static ReferralInvitation CreateReferralInvitation(long userId, long invitedByUserId)
    {
        return new ReferralInvitation
        {
            InvitedUserId = userId,
            InvitingUserId = invitedByUserId,
            InvitedAt = DateTimeOffset.Now
        };
    }

    private async Task ActivatePromocodeAsync(long userId, string text, CancellationToken stoppingToken)
    {
        var success = await promocodeService.TryActivatePromocodeAsync(userId, text, stoppingToken);
        if (success)
        {
            var replica = assetProvider.GetTextReplica(AssetKeys.Text.PromocodeActivationSuccess, out var parseMode);
            await bot.SendMessageAsync(userId, replica, parseMode, stoppingToken: stoppingToken);
        }
        else
        {
            var replica = assetProvider.GetTextReplica(AssetKeys.Text.PromocodeActivationFault, out var parseMode);
            await bot.SendMessageAsync(userId, replica, parseMode, stoppingToken: stoppingToken);
        }
    }

    private static DefaultUserState CreateNextState(UserCreationState state)
    {
        return new DefaultUserState
        {
            UserId = state.UserId,
            LastMessageId = state.LastMessageId
        };
    }
}
