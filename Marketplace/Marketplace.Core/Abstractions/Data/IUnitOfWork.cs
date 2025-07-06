namespace Marketplace.Core.Abstractions.Data;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IUserRepository UserRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    ISubscriptionRepository SubscriptionRepository { get; }
    IReferralInvitationRepository ReferralInvitationRepository { get; }
    IProjectTagRepository ProjectTagRepository { get; }
    IProjectRepository ProjectRepository { get; }
    IProjectCategoryRepository ProjectCategoryRepository { get; }
    ILikeRepository LikeRepository { get; }
    IComplaintRepository ComplaintRepository { get; }
}
