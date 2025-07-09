using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<ReferralInvitation> ReferralInvitations { get; set; }
    public DbSet<ProjectTag> ProjectTags { get; set; }
    public DbSet<ProjectCategory> ProjectCategories { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Complaint> Complaints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUsers(modelBuilder.Entity<User>());
        ConfigureTransactions(modelBuilder.Entity<Transaction>());
        ConfigureSubscriptions(modelBuilder.Entity<Subscription>());
        ConfigureReferralInvitations(modelBuilder.Entity<ReferralInvitation>());
        ConfigureProjectTag(modelBuilder.Entity<ProjectTag>());
        ConfigureProjectCategories(modelBuilder.Entity<ProjectCategory>());
        ConfigureProjects(modelBuilder.Entity<Project>());
        ConfigureLikes(modelBuilder.Entity<Like>());
        ConfigureComplaints(modelBuilder.Entity<Complaint>());
        ConfigurePromocodes(modelBuilder.Entity<Promocode>());
        ConfigurePromocodeActivations(modelBuilder.Entity<PromocodeActivation>());

        return;

        static void ConfigureUsers(EntityTypeBuilder<User> entity)
        {
            entity
                .ToTable("user")
                .HasKey(x => x.Id);
            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedNever(); // Don't generate value on add because it references Telegram chat id
            entity
                .Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(32)
                .IsRequired();
            entity
                .Property(x => x.SubscriptionId)
                .HasColumnName("subscription_id");
            entity
                .Property(x => x.IsAdmin)
                .HasColumnName("is_admin")
                .HasDefaultValue(false)
                .IsRequired();
            entity
                .Property(x => x.RegisteredAt)
                .HasColumnName("registered_at")
                .IsRequired();
            entity
                .Property(x => x.InvitationId)
                .HasColumnName("invitation_id");

            entity
                .HasIndex(x => x.Name)
                .HasDatabaseName("user_name_index")
                .IsUnique();
            entity
                .HasIndex(x => x.SubscriptionId)
                .HasDatabaseName("user_subscription_id_index")
                .IsUnique();

            entity
                .HasOne(x => x.Subscription)
                .WithOne(x => x.User)
                .HasForeignKey<User>(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
            entity
                .HasOne(x => x.Invitation)
                .WithOne(x => x.InvitedUser)
                .HasForeignKey<User>(x => x.Id)
                .OnDelete(DeleteBehavior.SetNull);
            /* entity
                .HasMany(x => x.MyInvitations)
                .WithOne(x => x.InvitingUser)
                .HasForeignKey(x => x.InvitingUserId)
                .OnDelete(DeleteBehavior.NoAction); */
        }

        static void ConfigureTransactions(EntityTypeBuilder<Transaction> entity)
        {
            entity
                .ToTable("transaction")
                .HasKey(x => x.Id);
            
            entity
                .Property(x => x.Id)
                .HasColumnName("id");
            entity
                .Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            entity
                .Property(x => x.Amount)
                .HasColumnName("amount")
                .IsRequired();
            entity
                .Property(x => x.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();
            entity
                .Property(x => x.Purpose)
                .HasColumnName("purpose")
                .IsRequired();
            entity
                .Property(x => x.Comment)
                .HasColumnName("comment")
                .HasMaxLength(512);
            entity
                .Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            entity
                .Property(x => x.FinishedAt)
                .HasColumnName("finished_at");
            entity
                .Property(x => x.Status)
                .HasColumnName("status")
                .HasDefaultValue(TransactionStatus.Created)
                .IsRequired();
            entity
                .Property(x => x.TelegramId)
                .HasColumnName("telegram_id");
            entity
                .Property(x => x.ProviderId)
                .HasColumnName("provider_id");

            entity
                .HasIndex(x => x.UserId)
                .HasDatabaseName("transaction_user_id_index");
            entity
                .HasIndex(x => x.TelegramId)
                .HasDatabaseName("transaction_telegram_id_index");
            
            entity
                .HasOne(x => x.User)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        static void ConfigureSubscriptions(EntityTypeBuilder<Subscription> entity)
        {
            entity
                .ToTable("subscription")
                .HasKey(x => x.Id);
            
            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity
                .Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            entity
                .Property(x => x.Active)
                .HasColumnName("active")
                .IsRequired();
            entity
                .Property(x => x.BaseActiveUntil)
                .HasColumnName("base_active_until");
            entity
                .Property(x => x.EnhancedUntil)
                .HasColumnName("enhanced_active_until");
                
            entity
                .HasIndex(x => x.UserId)
                .HasDatabaseName("subscription_user_id_index")
                .IsUnique();
            
            entity
                .HasOne(x => x.User)
                .WithOne(x => x.Subscription)
                .HasForeignKey<Subscription>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        static void ConfigureReferralInvitations(EntityTypeBuilder<ReferralInvitation> entity)
        {
            entity
                .ToTable("referral_invitation")
                .HasKey(x => x.Id);
            
            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity
                .Property(x => x.InvitingUserId)
                .HasColumnName("inviting_user_id")
                .IsRequired();
            entity
                .Property(x => x.InvitedUserId)
                .HasColumnName("invited_user_id")
                .IsRequired();
            entity
                .Property(x => x.InvitedAt)
                .HasColumnName("invited_at")
                .IsRequired();

            entity
                .HasIndex(x => x.InvitingUserId)
                .HasDatabaseName("ref_inviting_user_id_index");
            entity
                .HasIndex(x => x.InvitedUserId)
                .HasDatabaseName("ref_invited_user_id_index")
                .IsUnique();

            entity
                .HasOne(x => x.InvitedUser)
                .WithOne(x => x.Invitation)
                .HasForeignKey<ReferralInvitation>(x => x.InvitedUserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(x => x.InvitingUser)
                .WithMany(x => x.MyInvitations)
                .HasForeignKey(x => x.InvitingUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        static void ConfigureProjectTag(EntityTypeBuilder<ProjectTag> entity)
        {
            entity
                .ToTable("project_tag")
                .HasKey(x => x.Id);
            
            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity
                .Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(64)
                .IsRequired();
            entity
                .Property(x => x.CreatedByUserId)
                .HasColumnName("created_by_user_id");
            
            entity
                .HasIndex(x => x.Name)
                .HasDatabaseName("tag_name_index")
                .IsUnique();
            
            entity
                .HasOne(x => x.CreatedByUser)
                .WithMany(x => x.ProjectTags)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            /* entity
                .HasMany(x => x.Projects)
                .WithOne(x => x.Tag)
                .HasForeignKey(x => x.Id)
                .OnDelete(DeleteBehavior.NoAction); */
        }

        static void ConfigureProjectCategories(EntityTypeBuilder<ProjectCategory> entity)
        {
            entity
                .ToTable("category")
                .HasKey(x => x.Id);
            
            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity
                .Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(32)
                .IsRequired();
            entity
                .Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(512);
            
            entity
                .HasIndex(x => x.Name)
                .HasDatabaseName("category_name_index")
                .IsUnique();
            
            /* entity
                .HasMany(x => x.Projects)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.Id)
                .OnDelete(DeleteBehavior.NoAction); */
        }

        static void ConfigureProjects(EntityTypeBuilder<Project> entity)
        {
            entity
                .ToTable("project")
                .HasKey(x => x.Id);
            
            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity
                .Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(128)
                .IsRequired();
            entity
                .Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(512);
            entity
                .Property(x => x.ImageId)
                .HasColumnName("image_id");
            entity
                .Property(x => x.ContentUrl)
                .HasColumnName("content_url")
                .HasMaxLength(1024);
            entity
                .Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            entity
                .Property(x => x.Moderated)
                .HasColumnName("moderated")
                .HasDefaultValue(false)
                .IsRequired();
            entity
                .Property(x => x.Visible)
                .HasColumnName("visible")
                .HasDefaultValue(true)
                .IsRequired();
            entity
                .Property(x => x.CachedRating)
                .HasColumnName("cached_rating")
                .HasDefaultValue(0)
                .IsRequired();
            
            entity
                .HasIndex(x => x.Name)
                .HasDatabaseName("project_name_index")
                .IsUnique();
            entity
                .HasIndex(x => x.CategoryId)
                .HasDatabaseName("project_category_id_index");
            entity
                .HasIndex(x => x.TagId)
                .HasDatabaseName("project_tag_id_index");
            entity
                .HasIndex(x => x.UserId)
                .HasDatabaseName("project_user_id_index");
            entity
                .HasIndex(x => x.CachedRating)
                .HasDatabaseName("project_cached_rating_index");
            
            entity
                .HasOne(x => x.Category)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
            entity
                .HasOne(x => x.Tag)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.NoAction);
            entity
                .HasOne(x => x.User)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        static void ConfigureLikes(EntityTypeBuilder<Like> entity)
        {
            entity
                .ToTable("like")
                .HasKey(x => x.Id);
            
            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            
            entity
                .Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            entity
                .Property(x => x.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();
            entity
                .Property(x => x.Liked)
                .HasColumnName("liked")
                .IsRequired();
            
            entity
                .HasIndex(x => x.UserId)
                .HasDatabaseName("like_user_id_index");
            entity
                .HasIndex(x => x.ProjectId)
                .HasDatabaseName("like_project_id_index");
            entity
                .HasIndex(x => new { x.UserId, x.ProjectId })
                .HasDatabaseName("like_user_id_and_project_id_index")
                .IsUnique();
            
            entity
                .HasOne(x => x.Project)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(x => x.User)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        static void ConfigureComplaints(EntityTypeBuilder<Complaint> entity)
        {
            entity
                .ToTable("complaint")
                .HasKey(x => x.Id);
            
            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity
                .Property(x => x.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();
            entity
                .Property(x => x.ViolatorUserId)
                .HasColumnName("violator_user_id");
            entity
                .Property(x => x.ProjectId)
                .HasColumnName("project_id");
            entity
                .Property(x => x.Topic)
                .HasColumnName("topic")
                .IsRequired();
            entity
                .Property(x => x.Content)
                .HasColumnName("content")
                .HasMaxLength(1024)
                .IsRequired();
            entity
                .Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            entity
                .Property(x => x.ReviewedAt)
                .HasColumnName("reviewed_at");
            
            entity
                .HasIndex(x => x.ViolatorUserId)
                .HasDatabaseName("complaint_violator_user_id_index");
            entity
                .HasIndex(x => x.CreatedByUserId)
                .HasDatabaseName("complaint_created_by_user_id_index");
            entity
                .HasIndex(x => x.ProjectId)
                .HasDatabaseName("complaint_project_id_index");
            
            entity
                .HasOne(x => x.CreatedByUser)
                .WithMany(x => x.IssuedComplaints)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            entity
                .HasOne(x => x.ViolatorUser)
                .WithMany(x => x.AccusingComplaints)
                .HasForeignKey(x => x.ViolatorUserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(x => x.Project)
                .WithMany(x => x.Complaints)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        
        static void ConfigurePromocodes(EntityTypeBuilder<Promocode> entity)
        {
            entity
                .ToTable("promocode")
                .HasKey(x => x.Id);
        
            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity
                .Property(x => x.Text)
                .HasColumnName("text")
                .HasMaxLength(128)
                .IsRequired();
            entity
                .Property(x => x.ActiveUntil)
                .HasColumnName("active_until")
                .IsRequired();
            entity
                .Property(x => x.SubscriptionRenewDays)
                .HasColumnName("subscription_renew_days")
                .IsRequired();
            entity
                .Property(x => x.IsRenewEnhanced)
                .HasColumnName("is_renew_enhanced")
                .IsRequired();
        
            entity
                .HasIndex(x => x.Text)
                .HasDatabaseName("promocode_text_index")
                .IsUnique();
        }
        
        static void ConfigurePromocodeActivations(EntityTypeBuilder<PromocodeActivation> entity)
        {
            entity
                .ToTable("promocode_activation")
                .HasKey(x => x.Id);
        
            entity
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity
                .Property(x => x.PromocodeId)
                .HasColumnName("promocode_id")
                .IsRequired();
            entity
                .Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            entity
                .Property(x => x.ActivatedAt)
                .HasColumnName("activated_at")
                .IsRequired();
        
            entity
                .HasOne(x => x.User)
                .WithMany(x => x.PromocodeActivations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(x => x.Promocode)
                .WithMany(x => x.Activations)
                .HasForeignKey(x => x.PromocodeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
