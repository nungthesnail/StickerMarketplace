using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Logic.Models;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;
using Marketplace.Core.Models.Settings;
using Marketplace.Core.Models.UserStates;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Bot.Logic.Middlewares;

public class SubscriptionMiddleware(ILogger<SubscriptionMiddleware> logger, IExtendedBotClient bot,
    IAssetProvider assetProvider, IUserStateService userStateService, ISubscriptionService subscriptionService,
    IPromocodeService promocodeService, IControllerFactory controllerFactory, IStringParser stringParser,
    InvoiceSettings invoiceSettings, AppSettings appSettings) : AbstractMiddleware
{
    public override async Task InvokeAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken = default)
    {
        if (user is null)
            throw new NullReferenceException(nameof(user));

        if (!IsSubscriptionActiveAsync(user) && userState is not SubscriptionActivationState)
        {
            userState = new SubscriptionActivationState
            {
                UserId = user.Id
            };
            userStateService.SetUserState(user.Id, userState);
        }

        if (userState is SubscriptionActivationState activationState)
        {
            switch (activationState.Progress)
            {
                case SubscriptionActivationProgress.Introducing:
                    await IntroduceAsync(activationState, stoppingToken);
                    break;
                case SubscriptionActivationProgress.SelectRenewMethod:
                    await SelectRenewMethodAsync(activationState, update, stoppingToken);
                    break;
                case SubscriptionActivationProgress.PromocodeInput:
                    await PromocodeInputtedAsync(user, activationState, update, stoppingToken);
                    break;
                case SubscriptionActivationProgress.SelectPrice:
                    await SelectPriceAsync(user, activationState, update, stoppingToken);
                    break;
                case SubscriptionActivationProgress.InvoiceCreated:
                    await InvoiceCreatedAsync(activationState, update, stoppingToken);
                    logger.LogInformation("Created invoice. Payment method: {paymentMethod}, Price id: {price}",
                        activationState.RenewMethod, activationState.PriceIdx);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid progress value: {activationState.Progress}");
            }

            return;
        }
        
        await (Next?.InvokeAsync(user, userState, update, stoppingToken) ?? Task.CompletedTask);
        
        return;
        
        static bool IsSubscriptionActiveAsync(User user) => user.Subscription?.Active ?? false;
    }

    private async Task IntroduceAsync(SubscriptionActivationState userState, CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionBuyingIntroduction,
            out var parseMode, out var imageUrl, out _, out var effectId);
        var replyMarkup = CreateReplyMarkupForPaymentMethods();
        
        await bot.SendAsync(
            userState: userState,
            text: replica,
            parseMode: parseMode,
            photoUrl: imageUrl,
            replyMarkup: replyMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
        
        userState.Progress = SubscriptionActivationProgress.SelectRenewMethod;
        return;
        
        ReplyMarkup CreateReplyMarkupForPaymentMethods()
        {
            var labelPromocode = assetProvider.GetTextReplica(AssetKeys.Keyboards.SubscriptionForPromocode);
            var labelFriend = assetProvider.GetTextReplica(AssetKeys.Keyboards.SubscriptionForFriend);
            var labelStars = assetProvider.GetTextReplica(AssetKeys.Keyboards.SubscriptionForStars);

            return new InlineKeyboardMarkup
            {
                InlineKeyboard =
                [
                    [
                        new InlineKeyboardButton
                        {
                            CallbackData =
                                $"{AssetKeys.CallbackQueries.SubscriptionRenewMethod}/{SubscriptionRenewMethod.Promocode}",
                            Text = labelPromocode,
                            Pay = false
                        }
                    ],
                    [
                        new InlineKeyboardButton
                        {
                            CallbackData =
                                $"{AssetKeys.CallbackQueries.SubscriptionRenewMethod}/{SubscriptionRenewMethod.Friend}",
                            Text = labelFriend,
                            Pay = false
                        }
                    ],
                    [
                        new InlineKeyboardButton
                        {
                            CallbackData =
                                $"{AssetKeys.CallbackQueries.SubscriptionRenewMethod}/{SubscriptionRenewMethod.TelegramStars}",
                            Text = labelStars,
                            Pay = true
                        }
                    ]
                ]
            };
        }
    }

    private async Task SelectRenewMethodAsync(SubscriptionActivationState userState, Update update,
        CancellationToken stoppingToken = default)
    {
        if (update.CallbackQuery is not null
            && (update.CallbackQuery.Data?.StartsWith(AssetKeys.CallbackQueries.SubscriptionRenewMethod) ?? false))
        {
            var idxParam = stringParser.ExtractParameter(update.CallbackQuery?.Data, 1); // PaymentMethod/METHOD
            if (!Enum.TryParse<SubscriptionRenewMethod>(idxParam, true, out var renewMethod))
                throw new InvalidOperationException($"Invalid payment method: {idxParam}");
            userState.RenewMethod = renewMethod;

            switch (renewMethod)
            {
                case SubscriptionRenewMethod.Promocode:
                    await SendPromocodeInstructionsAsync(userState, stoppingToken);
                    break;
                case SubscriptionRenewMethod.Friend:
                    await SendFriendInstructionsAsync(userState, stoppingToken);
                    break;
                case SubscriptionRenewMethod.TelegramStars:
                    await SendStarsInstructionsAsync(userState, stoppingToken);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid renew method: {renewMethod}");
            }
        }
        else
        {
            var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionAwaitingRenewMethod,
                out var parseMode, out var imageUrl, out var replyMarkup, out var effectId);
            await bot.SendAsync(
                userState: userState,
                photoUrl: imageUrl,
                text: replica,
                parseMode: parseMode,
                replyMarkup: replyMarkup,
                messageEffectId: effectId,
                stoppingToken: stoppingToken);
        }
    }

    private async Task SendPromocodeInstructionsAsync(SubscriptionActivationState userState,
        CancellationToken stoppingToken)
    {
        userState.Progress = SubscriptionActivationProgress.PromocodeInput;
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionInputPromocode,
            out var parseMode, out var imageUrl, out _, out var effectId);
        await bot.SendAsync(
            userState,
            text: replica,
            parseMode: parseMode,
            photoUrl: imageUrl,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
    }

    private async Task PromocodeInputtedAsync(User? user, SubscriptionActivationState userState, Update update,
        CancellationToken stoppingToken)
    {
        if (update.Message?.Text is not null)
        {
            var promocode = update.Message.Text;
            var success = await promocodeService.TryActivatePromocodeAsync(userState.UserId, promocode, stoppingToken);
            if (!success)
            {
                var faultReplica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionWrongPromocode,
                    out var parseMode, out var imageUrl, out var replyMarkup, out var effectId);
                await bot.SendAsync(
                    userState: userState,
                    text: faultReplica,
                    parseMode: parseMode,
                    photoUrl: imageUrl,
                    replyMarkup: replyMarkup,
                    messageEffectId: effectId,
                    stoppingToken: stoppingToken);
                userState.Progress = SubscriptionActivationProgress.Introducing;
                await IntroduceAsync(userState, stoppingToken);
                return;
            }
            
            var replica = assetProvider.GetTextReplica(AssetKeys.Text.PromocodeActivationSuccess,
                out var parsingMode, out var photoUrl, out var reply, out var messageEffectId);
            await bot.SendAsync(
                userState: userState,
                text: replica,
                parseMode: parsingMode,
                photoUrl: photoUrl,
                replyMarkup: reply,
                messageEffectId: messageEffectId,
                stoppingToken: stoppingToken);

            var defaultState = DefaultUserState.Create();
            userStateService.SetUserState(userState.UserId, defaultState);
            if (user is not null)
                await WelcomeAsync(user, defaultState, update, stoppingToken);
        }
        else
        {
            var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionWaitingPromocode,
                out var parseMode, out var imageUrl, out var replyMarkup, out var effectId);
            await bot.SendAsync(
                userState: userState,
                text: replica,
                parseMode: parseMode,
                photoUrl: imageUrl,
                replyMarkup: replyMarkup,
                messageEffectId: effectId,
                stoppingToken: stoppingToken);
        }
    }

    private async Task WelcomeAsync(User user, DefaultUserState userState, Update update,
        CancellationToken stoppingToken)
    {
        var controller = CreateWelcomeController(user, userState, update);
        await controller.IntroduceAsync(stoppingToken);
    }
    
    private AbstractController CreateWelcomeController(User user, DefaultUserState userState, Update update)
    {
        var ctx = new ControllerContext(user, userState, update);
        var controller = controllerFactory.CreateController(ctx);
        if (controller is null)
            throw new InvalidOperationException("Cannot create welcome controller");
        return controller;
    }
    
    private async Task SendFriendInstructionsAsync(SubscriptionActivationState userState,
        CancellationToken stoppingToken)
    {
        userState.Progress = SubscriptionActivationProgress.FriendInvitation;
        var url = CreateUrl();
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionCopyInvitingLink,
            out var parseMode, out var imageUrl, out var replyMarkup, out var effectId,
            url);
        
        await bot.SendAsync(
            userState: userState,
            text: replica,
            parseMode: parseMode,
            photoUrl: imageUrl,
            replyMarkup: replyMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
        
        return;

        string CreateUrl() => $"{appSettings.AppUrl}/invitation-{userState.UserId}";
    }

    private async Task SendStarsInstructionsAsync(SubscriptionActivationState userState,
        CancellationToken stoppingToken)
    {
        var prices = subscriptionService.GetPrices();
        var pricesMarkup = CreatePricesMarkup();
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionSelectPrice,
            out var parseMode, out var imageUrl, out _, out var effectId);
        await bot.SendAsync(
            userState: userState,
            text: replica,
            parseMode: parseMode,
            photoUrl: imageUrl,
            replyMarkup: pricesMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
        
        userState.Progress = SubscriptionActivationProgress.SelectPrice;
        return;
        
        ReplyMarkup CreatePricesMarkup()
        {
            return new InlineKeyboardMarkup
            {
                InlineKeyboard = prices.Select((x, i) => new InlineKeyboardButton[]
                {
                    new()
                    {
                        CallbackData = $"{AssetKeys.CallbackQueries.PaymentPrice}/{i}",
                        Text = x.GetAsLabel(),
                        Pay = true
                    }
                })
            };
        }
    }

    private async Task SelectPriceAsync(User user, SubscriptionActivationState userState, Update update,
        CancellationToken stoppingToken)
    {
        if (update.CallbackQuery is not null
            && (update.CallbackQuery.Data?.StartsWith(AssetKeys.CallbackQueries.PaymentPrice) ?? false))
        {
            await CreateInvoiceAsync(user, userState, update, stoppingToken);
            userState.Progress = SubscriptionActivationProgress.InvoiceCreated;
        }
        else
        {
            var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionAwaitingPaymentPrice,
                out var parseMode, out var imageUrl, out var replyMarkup, out var effectId);
            await bot.SendAsync(
                userState: userState,
                photoUrl: imageUrl,
                text: replica,
                parseMode: parseMode,
                replyMarkup: replyMarkup,
                messageEffectId: effectId,
                stoppingToken: stoppingToken);
        }
    }

    private async Task CreateInvoiceAsync(User user, SubscriptionActivationState userState, Update update,
        CancellationToken stoppingToken)
    {
        const TransactionPurpose transactionPurpose = TransactionPurpose.SubscriptionRenewal;
        
        var priceIdx = int.Parse(stringParser.ExtractParameter(update.CallbackQuery?.Data, 1) ?? string.Empty); // PaymentPrice/IDX
        userState.PriceIdx = priceIdx;
        assetProvider.GetInvoice(AssetKeys.Invoices.Subscription,out var title, out var description, out var imageUrl);
        
        var price = subscriptionService.GetPrices()[priceIdx];
        var priceLabel = price.GetAsLabel();
        await bot.SendInvoiceAsync(
            chatId: user.Id,
            title: title,
            description: description,
            payload: transactionPurpose.ToString(),
            currency: userState.MethodAsCurrency(),
            photoUrl: imageUrl,
            prices:
            [
                new LabeledPrice { Label = priceLabel, Amount = price.Price }
            ],
            providerToken: invoiceSettings.ProviderToken,
            stoppingToken: stoppingToken);
    }

    private async Task InvoiceCreatedAsync(SubscriptionActivationState userState, Update update,
        CancellationToken stoppingToken)
    {
        if (update.Message?.Text is not null)
        {
            var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionAwaitingRenewMethod,
                out var parseMode, out var imageUrl, out var replyMarkup, out var effectId);
            await bot.SendAsync(
                userState: userState,
                photoUrl: imageUrl,
                text: replica,
                parseMode: parseMode,
                replyMarkup: replyMarkup,
                messageEffectId: effectId,
                stoppingToken: stoppingToken);
        }
    }
}
