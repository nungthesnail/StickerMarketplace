using Marketplace.Core.Models;

namespace Marketplace.Core.Abstractions.Services;

public interface IComplaintService
{
    Task CreateComplaintAsync(Complaint complaint, CancellationToken stoppingToken = default);

    Task<IEnumerable<Complaint>> GetAllAsync(bool onlyNotReviewed = false,
        CancellationToken stoppingToken = default);

    Task<IEnumerable<Complaint>> GetCreatedByUserIdAsync(long userId, bool onlyNotReviewed = false,
        CancellationToken stoppingToken = default);

    Task<IEnumerable<Complaint>> GetAccusingUserIdAsync(long userId, bool onlyNotReviewed = false,
        CancellationToken stoppingToken = default);

    Task<IEnumerable<Complaint>> GetAccusingProjectIdAsync(long projectId, bool onlyNotReviewed = false,
        CancellationToken stoppingToken = default);
}