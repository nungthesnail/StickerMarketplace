namespace Marketplace.Utils;

public delegate Task AsyncEventHandler(CancellationToken stoppingToken);
public delegate Task AsyncEventHandler<in TEventArgs>(TEventArgs args, CancellationToken stoppingToken);
