using Marketplace.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public abstract class AbstractMiddleware
{
    private AbstractMiddleware? _next;
    public AbstractMiddleware? Next
    {
        get => _next;
        set
        {
            if (_next == this)
                throw new InvalidOperationException("Can't make circular pipeline");
            _next = value;
        }
    }

    protected AbstractMiddleware(AbstractMiddleware? next)
    {
        Next = next;
    }
    
    protected AbstractMiddleware()
    { }
    
    public abstract Task InvokeAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken = default);
}
