using Marketplace.Core.Bot.Models;

namespace Marketplace.Core.Bot.Abstractions;

public interface IUpdatePipelineInvocator
{
    Task InvokePipelineAsync(Update update, CancellationToken stoppingToken = default);
}
