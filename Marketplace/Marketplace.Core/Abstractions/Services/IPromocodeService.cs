using Marketplace.Core.Models;

namespace Marketplace.Core.Abstractions.Services;

public interface IPromocodeService
{
    Task CreatePromocodeAsync(Promocode promocode, CancellationToken stoppingToken = default);
    Task<bool> TryActivatePromocodeAsync(long userId, string text, CancellationToken stoppingToken = default);
    Task<Promocode?> GetPromocodeAsync(string text, CancellationToken stoppingToken = default);
    Task<IEnumerable<Promocode>> GetAllPromocodesAsync(CancellationToken stoppingToken = default);
    Task<bool> PromocodeExistsAsync(string text, CancellationToken stoppingToken = default);
    Task<bool> UserActivatedPromocodeAsync(long userId, string text, CancellationToken stoppingToken = default);
}
