namespace Marketplace.Core.Bot.Logic;

public static class AssetKeys
{
    public static class Text
    {
        public const string UpdateHandlingFault = "UpdateHandlingFault";
        public const string Welcome = "Welcome";
        public const string InputName = "InputName";
        public const string CanReceiveOnlyText = "CanReceiveOnlyText";
        
        public const string InvalidUserName = "InvalidUserName";
        public const string UserNameIsNotAvailable = "UserNameIsNotAvailable";
        
        public const string PromocodeActivationFault = "PromocodeActivationFault";
        public const string PromocodeActivationSuccess = "PromocodeActivationSuccess";
        
        public const string SubscriptionBuyingIntroduction = "SubscriptionBuyingIntroduction";
        public const string SubscriptionAwaitingPaymentMethod = "SubscriptionAwaitingPaymentMethod";
        public const string SubscriptionInvoiceCreationFault = "SubscriptionInvoiceCreationFault";
        public const string SubscriptionAwaitingPayment = "SubscriptionAwaitingPayment";
    }

    public static class Commands
    {
        public const string Start = "/start";
    }

    public static class CallbackQueries
    {
        public const string PaymentMethod = "PaymentMethod";
    }

    public static class Invoices
    {
        public const string Subscription = "Subscription";
    }
}
