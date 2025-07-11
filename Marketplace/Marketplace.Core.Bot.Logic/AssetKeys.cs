namespace Marketplace.Core.Bot.Logic;

public static class AssetKeys
{
    public static class Text
    {
        public const string UpdateHandlingFault = "UpdateHandlingFault";
        
        public const string Welcome = "Welcome";
        public const string WelcomeGoToCatalog = "WelcomeGoToCatalog";
        public const string WelcomeCreateContent = "WelcomeCreateContent";
        public const string WelcomeMyProfile = "WelcomeMyProfile";
        
        public const string InputName = "InputName";
        public const string CanReceiveOnlyText = "CanReceiveOnlyText";
        
        public const string InvalidUserName = "InvalidUserName";
        public const string UserNameIsNotAvailable = "UserNameIsNotAvailable";
        
        public const string PromocodeActivationFault = "PromocodeActivationFault";
        public const string PromocodeActivationSuccess = "PromocodeActivationSuccess";
        
        public const string SubscriptionBuyingIntroduction = "SubscriptionBuyingIntroduction";
        public const string SubscriptionAwaitingRenewMethod = "SubscriptionAwaitingPaymentMethod";
        public const string SubscriptionAwaitingPaymentPrice = "SubscriptionAwaitingPaymentPrice";
        public const string SubscriptionSelectPrice = "SubscriptionSelectPrice";
        public const string SubscriptionInputPromocode = "SubscriptionInputPromocode";
        public const string SubscriptionWaitingPromocode = "WaitingPromocode";
        public const string SubscriptionWrongPromocode = "SubscriptionWrongPromocode";
        public const string SubscriptionCopyInvitingLink = "SubscriptionCopyInvitingLink";
        public const string SubscriptionRenewed = "SubscriptionRenewed";
        
        public const string TransactionProcessingFailed = "TransactionProcessingFailed";
        
        public const string CatalogFilterSelectCategory = "CatalogFilterTags";
        public const string CatalogFilterAwaitingCategory = "CatalogFilterAwaitingCategory";
        public const string CatalogFilterSelectTags = "CatalogFilterTags";
        public const string CatalogFilterAwaitingTags = "CatalogFilterAwaitingTags";
        
        public const string CallbackQueryParseError = "CallbackQueryParseError";
    }

    public static class Commands
    {
        public const string Start = "/start";
    }

    public static class CallbackQueries
    {
        public const string SubscriptionRenewMethod = "SubscriptionRenewMethod";
        public const string PaymentPrice = "PaymentPrice";
    }

    public static class Invoices
    {
        public const string Subscription = "Subscription";
    }

    public static class Keyboards
    {
        public const string SubscriptionForPromocode = "SubscriptionForPromocode";
        public const string SubscriptionForFriend = "SubscriptionForFriend";
        public const string SubscriptionForStars = "SubscriptionForStars";
        
        public const string CatalogFilterTagsSelected = "CatalogFilterTagsSelected";
    }
}
