using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums.UserStates;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Logic.Middlewares;

public partial class RegistrationMiddleware(IUserStateService userStateService, IAssetProvider assetProvider,
    IBotClient botClient, IUserService userService,
    IReferralInvitationService referalService) : AbstractMiddleware
{
    public override async Task InvokeAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken = default)
    {
        if (update.Message is not null)
        {
            var message = update.Message;
            user = await userService.GetUserByIdAsync(message.Chat.Id, stoppingToken);
            userState = userStateService.GetUserState(message.Chat.Id);
            if (user is null && (userState is null || userState is UserCreationState))
            {
                await ProcessUserCreationAsync((UserCreationState?)userState, update, stoppingToken);
                return;
            }
            if (IsStartMessage(update))
                await SendHelloMessageAsync(update.Message!.Chat.Id, stoppingToken);
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
                await CompleteUserCreationAsync(userState, stoppingToken);
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
        await SendHelloMessageAsync(userId, stoppingToken);
        await SendSetNameMessageAsync(userId, stoppingToken);

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

    private async Task SendHelloMessageAsync(long userId, CancellationToken stoppingToken = default)
    {
        await botClient.SendMessageAsync(
            chatId: userId,
            text: assetProvider.GetTextReplica(AssetKeys.Text.Welcome, out var parseMode),
            parseMode: parseMode,
            stoppingToken: stoppingToken);
    }

    private async Task SendSetNameMessageAsync(long userId, CancellationToken stoppingToken = default)
    {
        await botClient.SendMessageAsync(
            chatId: userId,
            text: assetProvider.GetTextReplica(AssetKeys.Text.InputName, out var parseMode),
            parseMode: parseMode,
            stoppingToken: stoppingToken);
    }

    private async Task SetUserNameAsync(UserCreationState state, Update update, CancellationToken stoppingToken)
    {
        if (!ValidateUserName(update, out var userName))
        {
            await SendInvalidUserNameMessageAsync(update.Message!.Chat.Id, stoppingToken);
            return;
        }
        var userNameAvailable = await IsUserNameAvailableAsync(userName, stoppingToken);
        if (!userNameAvailable)
        {
            await SendUserNameUnavailableMessageAsync(update.Message!.Chat.Id, stoppingToken);
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
        return regex.IsMatch(userName) && userName.Length <= maxNameLength && userName.Length >= minNameLength;
    }

    [GeneratedRegex("^[a-zA-Z0-9_-]+$")] // Todo: check regex
    private static partial Regex UserNameRegex();

    private async Task SendInvalidUserNameMessageAsync(long userId, CancellationToken stoppingToken)
    {
        await botClient.SendMessageAsync(
            chatId: userId,
            text: assetProvider.GetTextReplica(AssetKeys.Text.InvalidUserName, out var parseMode),
            parseMode: parseMode,
            stoppingToken: stoppingToken);
    }

    private async Task<bool> IsUserNameAvailableAsync(string userName, CancellationToken stoppingToken)
    {
        return await userService.IsNameAvailableAsync(userName, stoppingToken);
    }
    
    private async Task SendUserNameUnavailableMessageAsync(long userId, CancellationToken stoppingToken)
    {
        await botClient.SendMessageAsync(
            chatId: userId,
            text: assetProvider.GetTextReplica(AssetKeys.Text.UserNameIsNotAvailable, out var parseMode),
            parseMode: parseMode,
            stoppingToken: stoppingToken);
    }

    private async Task CompleteUserCreationAsync(UserCreationState state, CancellationToken stoppingToken)
    {
        if (state.UserName is null)
            throw new InvalidOperationException("User name must be provided to create user");
        var userNameAvailable = await userService.IsNameAvailableAsync(state.UserName, stoppingToken);
        if (!userNameAvailable)
        {
            await SendUserNameUnavailableMessageAsync(state.UserId, stoppingToken);
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
            await referalService.CreateInvitationAsync(
                invitation: invitation,
                openTransaction: true,
                updateInvitations: true,
                stoppingToken: stoppingToken);
        }

        if (state.PromoCode is not null)
        {
            // In future here will be called the promocode service
        }
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
}
