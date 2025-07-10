namespace Marketplace.Core.Models.Enums.UserStates;

public enum SubscriptionActivationProgress
{
    Introducing,
    SelectRenewMethod,
    SelectPrice,
    PromocodeInput,
    FriendInvitation,
    InvoiceCreated
}

public static class SubscriptionActivationProgressExtensions
{
    public static SubscriptionActivationProgress GetNext(this SubscriptionActivationProgress state)
    {
        return state switch
        {
            SubscriptionActivationProgress.SelectRenewMethod => SubscriptionActivationProgress.InvoiceCreated,
            SubscriptionActivationProgress.InvoiceCreated => SubscriptionActivationProgress.InvoiceCreated,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    public static SubscriptionActivationProgress GetPrevious(this SubscriptionActivationProgress state)
    {
        return state switch
        {
            SubscriptionActivationProgress.InvoiceCreated => SubscriptionActivationProgress.SelectRenewMethod,
            SubscriptionActivationProgress.SelectRenewMethod => SubscriptionActivationProgress.SelectRenewMethod,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}
