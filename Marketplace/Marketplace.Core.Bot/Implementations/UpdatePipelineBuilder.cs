using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Implementations.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Core.Bot.Implementations;

public class UpdatePipelineBuilder : IUpdatePipelineBuilder
{
    private readonly List<Type> _pipelineMiddlewares = [typeof(UpdatePipelineMiddleware)];
    
    public void AddMiddleware<TMiddleware>() where TMiddleware : AbstractMiddleware
    {
        var type = typeof(TMiddleware);
        if (_pipelineMiddlewares.Contains(type))
            throw new InvalidOperationException("Can't create circular pipeline");
    }

    public AbstractMiddleware BuildPipeline(IServiceProvider serviceProvider)
    {
        try
        {
            var middlewares = _pipelineMiddlewares
                .Select(type => (AbstractMiddleware)serviceProvider.GetRequiredService(type));
            
            UpdatePipelineMiddleware? first = null;
            AbstractMiddleware? last = null;
            foreach (var middleware in middlewares)
            {
                if (last is null)
                    last = middleware;
                else
                    last = last.Next = middleware;
                first ??= (UpdatePipelineMiddleware)last;
            }

            return first ?? throw new InvalidOperationException("The pipeline is empty");
        }
        catch (InvalidCastException exc)
        {
            throw new InvalidOperationException("The pipeline is corrupted", exc);
        }
    }
}
