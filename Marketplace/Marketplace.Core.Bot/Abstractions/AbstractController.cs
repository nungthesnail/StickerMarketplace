using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public abstract class AbstractController(IControllerContext ctx)
{
    private readonly IControllerFactory? _controllerFactory;
    private readonly IUserStateService? _userStateService;
    
    protected IControllerContext Context { get; } = ctx;
    protected Update Update { get; } = ctx.Update;
    protected UserState BaseUserState { get; } = ctx.UserState;

    protected AbstractController(IControllerContext ctx, IControllerFactory controllerFactory)
        : this(ctx)
    {
        _controllerFactory = controllerFactory;
    }
    
    protected AbstractController(IControllerContext ctx, IControllerFactory controllerFactory,
        IUserStateService userStateService)
        : this(ctx)
    {
        _controllerFactory = controllerFactory;
        _userStateService = userStateService;
    }
    
    public abstract Task HandleUpdateAsync(CancellationToken stoppingToken = default);
    public virtual Task IntroduceAsync(CancellationToken stoppingToken = default) => Task.CompletedTask;

    protected AbstractController CreateController(UserState state)
    {
        if (_controllerFactory is null)
            throw new InvalidOperationException("Controller factory is not provided");
        
        var ctx = Context.CopyWithState(state);
        var controller = _controllerFactory.CreateController(ctx);
        return controller ?? throw new NullReferenceException($"Cannot create controller for {state.GetType().Name}");
    }

    protected TNewState CreateUserState<TNewState>()
        where TNewState : UserState, new()
    {
        return new TNewState
        {
            UserId = BaseUserState.UserId,
            LastMessageId = BaseUserState.LastMessageId
        };
    }
    
    protected async Task ChangeToNewStateAsync<TNewState>(TNewState? state = null,
        CancellationToken stoppingToken = default)
        where TNewState : UserState, new()
    {
        if (_userStateService is null)
            throw new InvalidOperationException("User state service is not provided");
        
        state ??= CreateUserState<TNewState>();
        _userStateService.SetUserState(state.UserId, state);
        var controller = CreateController(state);
        await controller.IntroduceAsync(stoppingToken);
    }
}

public abstract class AbstractController<TUserState> : AbstractController
    where TUserState : UserState
{
    protected AbstractController(IControllerContext ctx)
        : base(ctx)
    { }
    
    protected AbstractController(IControllerContext ctx, IControllerFactory controllerFactory)
        : base(ctx, controllerFactory)
    { }
    
    protected AbstractController(IControllerContext ctx, IControllerFactory controllerFactory,
        IUserStateService userStateService) : base(ctx, controllerFactory, userStateService)
    { }
    
    private TUserState? _userState;
    protected TUserState UserState => _userState ??= (TUserState)BaseUserState;
    
    protected User User => Context.User;
}
