using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Logic.Models;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;
using Marketplace.Core.Models.UserStates;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Bot.Logic.Middlewares;

public class SubscriptionMiddleware(ILogger<SubscriptionMiddleware> logger, IExtendedBotClient bot,
    IAssetProvider assetProvider, IUserStateService userStateService, ISubscriptionService subscriptionService,
    IStringParser stringParser, InvoiceSettings invoiceSettings) : AbstractMiddleware
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
                    await IntroduceAsync(activationState, update, stoppingToken);
                    break;
                case SubscriptionActivationProgress.SelectPaymentMethod:
                    await SelectPaymentMethodAsync(user, activationState, update, stoppingToken);
                    break;
                case SubscriptionActivationProgress.InvoiceCreated:
                    await InvoiceCreatedAsync(activationState, update, stoppingToken);
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

    private async Task IntroduceAsync(SubscriptionActivationState userState, Update update,
        CancellationToken stoppingToken)
    {
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionBuyingIntroduction,
            out var parseMode, out var imageUrl, out var replyMarkup, out var effectId);
        await bot.SendAsync(
            userState: userState,
            photoUrl: imageUrl,
            text: replica,
            parseMode: parseMode,
            replyMarkup: replyMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);
        userState.MoveProgressNext();
    }

    private async Task SelectPaymentMethodAsync(User user, SubscriptionActivationState userState, Update update,
        CancellationToken stoppingToken)
    {
        if (update.CallbackQuery is not null
            && (update.CallbackQuery.Data?.StartsWith(AssetKeys.CallbackQueries.PaymentMethod) ?? false))
        {
            await CreateInvoiceAsync(user, userState, update, stoppingToken);
            userState.MoveProgressNext();
        }
        else
        {
            var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionAwaitingPaymentMethod,
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
        
        // Preparing data for invoice creation
        var paymentMethod = stringParser.ExtractParameter(update.CallbackQuery?.Data, 1); // PaymentMethod/METHOD/PRICE_IDX
        if (paymentMethod is null)
            throw new InvalidOperationException($"Invalid payment method: {update.CallbackQuery?.Data}");

        if (!Enum.TryParse<TransactionMethod>(paymentMethod, true, out var transactionMethod))
            throw new InvalidOperationException($"Invalid payment method: {update.CallbackQuery?.Data}");

        var priceIdx = int.Parse(stringParser.ExtractParameter(update.CallbackQuery?.Data, 2) ?? string.Empty);
        userState.PriceIdx = priceIdx;

        userState.Method = transactionMethod;
        assetProvider.GetInvoice(AssetKeys.Invoices.Subscription, out var title, out var description, out var imageUrl);
        
        var price = subscriptionService.GetPrices()[priceIdx];
        var priceLabel = price.GetAsLabel();
        
        // Sending invoice
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
            var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionAwaitingPaymentMethod,
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
