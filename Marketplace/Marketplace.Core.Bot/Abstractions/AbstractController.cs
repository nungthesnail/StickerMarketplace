using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Implementations;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Core.Bot.Abstractions;

public abstract class AbstractController(IControllerContext ctx)
{
    private readonly IControllerFactory? _controllerFactory;
    private readonly IUserStateService? _userStateService;
    private readonly IServiceProvider? _serviceProvider;
    
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

    protected AbstractController(IControllerContext ctx, IControllerFactory controllerFactory,
        IUserStateService userStateService, IServiceProvider services) : this(ctx, controllerFactory, userStateService)
    {
        _serviceProvider = services;
    }
    
    public abstract Task HandleUpdateAsync(CancellationToken stoppingToken = default);
    public virtual Task IntroduceAsync(CancellationToken stoppingToken = default) => Task.CompletedTask;

    protected AbstractController? CreateController(UserState state)
    {
        if (_controllerFactory is null)
            throw new InvalidOperationException("Controller factory is not provided");
        
        var context = Context.CopyWithState(state);
        return _controllerFactory.CreateController(context);
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
        if (controller is not null)
        {
            await controller.IntroduceAsync(stoppingToken);
        }
        else if (_serviceProvider is not null)
        {
            var pipeline = _serviceProvider.GetRequiredService<UpdatePipelineMiddleware>();
            await pipeline.InvokeAsync(ctx.User, ctx.UserState, ctx.Update, stoppingToken);
        }
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

    protected AbstractController(IControllerContext ctx, IControllerFactory controllerFactory,
        IUserStateService userStateService, IServiceProvider services)
        : base(ctx, controllerFactory, userStateService, services)
    { }
    
    private TUserState? _userState;
    protected TUserState UserState => _userState ??= (TUserState)BaseUserState;
    
    protected User User => Context.User;
}
