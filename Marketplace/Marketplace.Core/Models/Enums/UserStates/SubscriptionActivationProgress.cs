namespace Marketplace.Core.Models.Enums.UserStates;

public enum SubscriptionActivationProgress
{
    Introducing,
    SelectPaymentMethod,
    InvoiceCreated
}

public static class SubscriptionActivationProgressExtensions
{
    public static SubscriptionActivationProgress GetNext(this SubscriptionActivationProgress state)
    {
        return state switch
        {
            SubscriptionActivationProgress.SelectPaymentMethod => SubscriptionActivationProgress.InvoiceCreated,
            SubscriptionActivationProgress.InvoiceCreated => SubscriptionActivationProgress.InvoiceCreated,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    public static SubscriptionActivationProgress GetPrevious(this SubscriptionActivationProgress state)
    {
        return state switch
        {
            SubscriptionActivationProgress.InvoiceCreated => SubscriptionActivationProgress.SelectPaymentMethod,
            SubscriptionActivationProgress.SelectPaymentMethod => SubscriptionActivationProgress.SelectPaymentMethod,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}
