namespace Marketplace.Core.Bot.Abstractions;

public interface IUpdatePipelineBuilder
{
    void AddMiddleware<TMiddleware>() where TMiddleware : AbstractMiddleware;
    AbstractMiddleware BuildPipeline(IServiceProvider serviceProvider);
}
