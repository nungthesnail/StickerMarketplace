using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;

namespace Marketplace.EntityFrameworkCore.Implementations;

public class EfUserRepository(AppDbContext dbContext)
    : AbstractEfRepository<User, long>(dbContext), IUserRepository;

public class EfTransactionRepository(AppDbContext dbContext)
    : AbstractEfRepository<Transaction, Guid>(dbContext), ITransactionRepository;

public class EfSubscriptionRepository(AppDbContext dbContext)
    : AbstractEfRepository<Subscription, long>(dbContext), ISubscriptionRepository;

public class EfReferralInvitationRepository(AppDbContext dbContext)
    : AbstractEfRepository<ReferralInvitation, long>(dbContext), IReferralInvitationRepository;
    
public class EfProjectTagRepository(AppDbContext dbContext)
    : AbstractEfRepository<ProjectTag, long>(dbContext), IProjectTagRepository;
    
public class EfProjectRepository(AppDbContext dbContext)
    : AbstractEfRepository<Project, long>(dbContext), IProjectRepository;

public class EfProjectCategoryRepository(AppDbContext dbContext)
    : AbstractEfRepository<ProjectCategory, CategoryIdentifier>(dbContext), IProjectCategoryRepository;

public class EfLikeRepository(AppDbContext dbContext)
    : AbstractEfRepository<Like, long>(dbContext), ILikeRepository;

public class EfComplaintRepository(AppDbContext dbContext)
    : AbstractEfRepository<Complaint, long>(dbContext), IComplaintRepository;

public class EfPromocodeRepository(AppDbContext dbContext)
    : AbstractEfRepository<Promocode, long>(dbContext), IPromocodeRepository;

public class EfPromocodeActivationRepository(AppDbContext dbContext)
    : AbstractEfRepository<PromocodeActivation, long>(dbContext), IPromocodeActivationRepository;
