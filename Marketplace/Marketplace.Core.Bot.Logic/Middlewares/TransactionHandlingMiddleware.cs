using Marketplace.Core.Abstractions.Services;
using Marketplace.Bot.Abstractions;
using Marketplace.Bot.Models;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Logic.Exceptions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.UserStates;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Bot.Logic.Middlewares;

public class TransactionHandlingMiddleware(IBotClient bot, ITransactionService transactionService,
    ISubscriptionService subscriptionService, IAssetProvider assetProvider, IUserStateService userStateService,
    IControllerFactory controllerFactory, ILogger<TransactionHandlingMiddleware> logger) : AbstractMiddleware
{
    public override async Task InvokeAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken = default)
    {
        if (update.PreCheckoutQuery is not null)
        {
            await AnswerPreCheckoutQuery(update.PreCheckoutQuery, stoppingToken);
            return;
        }
        if (update.Message?.SuccessfulPayment is not null)
        {
            await ProcessSuccessfulPaymentAsync(user, update.Message.SuccessfulPayment, update, stoppingToken);
            return;
        }
        
        await (Next?.InvokeAsync(user, userState, update, stoppingToken) ?? Task.CompletedTask);
    }

    private async Task AnswerPreCheckoutQuery(PreCheckoutQuery preCheckoutQuery, CancellationToken stoppingToken)
    {
        await bot.AnswerPreCheckoutQueryAsync(preCheckoutQuery.Id, stoppingToken: stoppingToken);
    }
    
    private async Task ProcessSuccessfulPaymentAsync(User? user, SuccessfulPayment payment, Update update,
        CancellationToken stoppingToken)
    {
        var transactionId = Guid.NewGuid();
        try
        {
            if (!Enum.TryParse(payment.InvoicePayload, out TransactionPurpose purpose))
            {
                purpose = TransactionPurpose.Unknown;
                logger.LogError("Invalid payment purpose: {purpose}", payment.InvoicePayload);
            }

            await transactionService.CreateTransactionAsync(new Transaction
            {
                Id = transactionId,
                TelegramId = payment.TelegramPaymentChargeId,
                Amount = payment.TotalAmount,
                Currency = payment.Currency,
                CreatedAt = DateTimeOffset.Now,
                Purpose = purpose,
                Status = TransactionStatus.Created,
                UserId = update.Message!.Chat.Id
            }, stoppingToken);

            switch (purpose)
            {
                case TransactionPurpose.SubscriptionRenewal:
                    await ProcessSubscriptionRenewalAsync(user, payment, update, stoppingToken);
                    break;
                case TransactionPurpose.Unknown:
                default:
                    throw new UndefinedTransactionPurposeException(purpose.ToString());
            }
            
            await transactionService.UpdateTransactionStatusAsync(
                id: transactionId,
                status: TransactionStatus.Success,
                stoppingToken: stoppingToken);
        }
        catch (Exception exc)
        {
            logger.LogCritical(exc, "Something failed while processing payment. Telegram transaction id = {tgId}" +
                                    "Internal transaction id = {transactionId}",
                payment.TelegramPaymentChargeId, transactionId);
            
            var replica = assetProvider.GetTextReplica(AssetKeys.Text.TransactionProcessingFailed,
                out var parseMode, out var imageUrl, out var replyMarkup, out var effectId);
            await bot.SendAsync(
                chatId: update.Message!.Chat.Id,
                text: replica,
                parseMode: parseMode,
                photoUrl: imageUrl,
                replyMarkup: replyMarkup,
                messageEffectId: effectId,
                stoppingToken: stoppingToken);
            
            throw;
        }
    }

    private async Task ProcessSubscriptionRenewalAsync(User? user, SuccessfulPayment payment, Update update,
        CancellationToken stoppingToken)
    {
        var userId = update.Message!.Chat.Id;
        
        var prices = subscriptionService.GetPrices();
        var priceInfo = prices.FirstOrDefault(x => x.Price == payment.TotalAmount
                                                   && x.Currency.ToString() == payment.Currency);
        if (priceInfo is null)
            throw new UnmatchedPriceTransactionException($"Price: {payment.TotalAmount}, Currency: {payment.Currency}");

        var renewInterval = TimeSpan.FromDays(priceInfo.DayCount);
        var enhanced = priceInfo.Enhanced;
        await subscriptionService.RenewSubscriptionByUserIdAsync(
            userId: userId,
            timeSpan: renewInterval,
            enhanced: enhanced,
            stoppingToken: stoppingToken);
        
        var replica = assetProvider.GetTextReplica(AssetKeys.Text.SubscriptionRenewed,
            out var parseMode, out var imageUrl, out var replyMarkup, out var effectId,
            priceInfo.DayCount);
        await bot.SendAsync(
            chatId: userId,
            text: replica,
            parseMode: parseMode,
            photoUrl: imageUrl,
            replyMarkup: replyMarkup,
            messageEffectId: effectId,
            stoppingToken: stoppingToken);

        var defaultState = DefaultUserState.Create();
        userStateService.SetUserState(userId, defaultState);
        if (user is not null)
            await WelcomeAsync(user, defaultState, update, stoppingToken);
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
}
