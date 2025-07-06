namespace Marketplace.Core.Models.Settings;

public record ReferralInvitationSettings(int MinCountToRenewSubscription, double SubscriptionRenewDays,
    bool IsSubscriptionEnhanced);
