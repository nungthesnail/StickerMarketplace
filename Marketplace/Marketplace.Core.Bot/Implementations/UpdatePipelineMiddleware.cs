using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Bot.Implementations;

public class UpdatePipelineMiddleware(AbstractMiddleware? next, IUnitOfWork uow,
    ILogger<UpdatePipelineMiddleware> logger) : AbstractMiddleware(next)
{
    public override async Task InvokeAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken = default)
    {
        try
        {
            logger.LogTrace("Starting handling update id={id}", update.Id);
            await (Next?.InvokeAsync(user, userState, update, stoppingToken) ?? Task.CompletedTask);
            logger.LogTrace("Successfully finished handling update id={id}", update.Id);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Something failed while handling update id={id}", update.Id);
            if (uow.IsTransactionOpened)
            {
                logger.LogTrace("Rollback transaction on failed update id={id}", update.Id);
                await uow.RollbackTransactionAsync(stoppingToken);
            }
        }
    }
}
