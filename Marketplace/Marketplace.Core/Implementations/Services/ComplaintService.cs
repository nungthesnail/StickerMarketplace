using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Models;

namespace Marketplace.Core.Implementations.Services;

public class ComplaintService(IUnitOfWork uow) : IComplaintService
{
    public async Task CreateComplaintAsync(Complaint complaint, CancellationToken stoppingToken = default)
    {
        await uow.ComplaintRepository.AddAsync(complaint, stoppingToken);
        await uow.SaveChangesAsync(stoppingToken);
    }

    public async Task<IEnumerable<Complaint>> GetAllAsync(bool onlyNotReviewed = false,
        CancellationToken stoppingToken = default)
        => await uow.ComplaintRepository.GetByAsync(
            predicate: x => (!x.Reviewed && onlyNotReviewed) || !onlyNotReviewed,
            stoppingToken: stoppingToken);

    public async Task<IEnumerable<Complaint>> GetCreatedByUserIdAsync(long userId, bool onlyNotReviewed = false,
        CancellationToken stoppingToken = default)
        => await uow.ComplaintRepository.GetByAsync(
            predicate: x => x.CreatedByUserId == userId && ((onlyNotReviewed && !x.Reviewed) || !onlyNotReviewed),
            stoppingToken: stoppingToken);
    
    public async Task<IEnumerable<Complaint>> GetAccusingUserIdAsync(long userId, bool onlyNotReviewed = false,
        CancellationToken stoppingToken = default)
        => await uow.ComplaintRepository.GetByAsync(
            predicate: x => x.ViolatorUserId == userId && ((onlyNotReviewed && !x.Reviewed) || !onlyNotReviewed),
            stoppingToken: stoppingToken);
    
    public async Task<IEnumerable<Complaint>> GetAccusingProjectIdAsync(long projectId, bool onlyNotReviewed = false,
        CancellationToken stoppingToken = default)
        => await uow.ComplaintRepository.GetByAsync(
            predicate: x => x.ProjectId == projectId && ((onlyNotReviewed && !x.Reviewed) || !onlyNotReviewed),
            stoppingToken: stoppingToken);
}
