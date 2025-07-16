using Marketplace.Bot.Models;
using Marketplace.Core.Bot.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Core.Bot.Implementations;

public class UpdatePipelineInvocator(IServiceProvider serviceProvider) : IUpdatePipelineInvocator
{
    public async Task InvokePipelineAsync(Update update, CancellationToken stoppingToken = default)
    {
        var pipelineBuilder = serviceProvider.GetRequiredService<IUpdatePipelineBuilder>();
        var pipeline = pipelineBuilder.BuildPipeline(serviceProvider);
        await pipeline.InvokeAsync(null, null, update, stoppingToken);
    }
}
