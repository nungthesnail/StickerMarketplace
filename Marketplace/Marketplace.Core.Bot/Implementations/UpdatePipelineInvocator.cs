using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Core.Bot.Implementations;

public class UpdatePipelineInvocator(IServiceProvider serviceProvider) : IUpdatePipelineInvocator
{
    public async Task InvokePipelineAsync(Update update, CancellationToken stoppingToken = default)
    {
        await using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
        var pipelineBuilder = serviceProvider.GetRequiredService<UpdatePipelineBuilder>();
        var pipeline = pipelineBuilder.BuildPipeline(scope.ServiceProvider);
        await pipeline.InvokeAsync(null, null, update, stoppingToken);
    }
}
