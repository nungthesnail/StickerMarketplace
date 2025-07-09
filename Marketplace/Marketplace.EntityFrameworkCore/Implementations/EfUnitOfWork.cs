using Marketplace.Core.Abstractions.Data;

namespace Marketplace.EntityFrameworkCore.Implementations;

public class EfUnitOfWork(AppDbContext dbContext) : EfBaseUnitOfWork(dbContext), IUnitOfWork
{
    private EfUserRepository? _userRepository;
    public IUserRepository UserRepository
        => _userRepository ??= new EfUserRepository(dbContext);

    private EfTransactionRepository? _transactionRepository;
    public ITransactionRepository TransactionRepository
        => _transactionRepository ??= new EfTransactionRepository(dbContext);
    
    private EfSubscriptionRepository? _subscriptionRepository;
    public ISubscriptionRepository SubscriptionRepository
        => _subscriptionRepository ??= new EfSubscriptionRepository(dbContext);
    
    private EfReferralInvitationRepository? _referralInvitationRepository;
    public IReferralInvitationRepository ReferralInvitationRepository
        => _referralInvitationRepository ??= new EfReferralInvitationRepository(dbContext);
    
    private EfProjectTagRepository? _projectTagRepository;
    public IProjectTagRepository ProjectTagRepository
        => _projectTagRepository ??= new EfProjectTagRepository(dbContext);
    
    private EfProjectRepository? _projectRepository;
    public IProjectRepository ProjectRepository
        => _projectRepository ??= new EfProjectRepository(dbContext);
    
    private EfProjectCategoryRepository? _projectCategoryRepository;
    public IProjectCategoryRepository ProjectCategoryRepository
        => _projectCategoryRepository ??= new EfProjectCategoryRepository(dbContext);
    
    private EfLikeRepository? _likeRepository;
    public ILikeRepository LikeRepository
        => _likeRepository ??= new EfLikeRepository(dbContext);
    
    private EfComplaintRepository? _complaintRepository;
    public IComplaintRepository ComplaintRepository
        => _complaintRepository ??= new EfComplaintRepository(dbContext);

    private EfPromocodeRepository? _promocodeRepository;
    public IPromocodeRepository PromocodeRepository
        => _promocodeRepository ??= new EfPromocodeRepository(dbContext);
    
    private EfPromocodeActivationRepository? _promocodeActivationRepository;
    public IPromocodeActivationRepository PromocodeActivationRepository
        => _promocodeActivationRepository ??= new EfPromocodeActivationRepository(dbContext);
}
