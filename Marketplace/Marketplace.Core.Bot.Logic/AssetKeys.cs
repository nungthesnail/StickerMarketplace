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
        
        public const string CatalogNoProjectsFound = "CatalogNoProjectsFound";
        public const string CatalogProjectView = "CatalogProjectView";
        
        public const string CreationSetCategory = "CreationSetCategory";
        public const string CreationSetName = "CreationSetName";
        public const string CreationNameIsTooLong = "CreationNameIsTooLong";
        public const string CreationNameIsNotAvailable = "CreationNameIsNotAvailable";
        public const string CreationSetDescription = "CreationSetDescription";
        public const string CreationDescriptionIsTooLong = "CreationDescriptionIsTooLong";
        public const string CreationUploadImage = "CreationUploadImage";
        public const string CreationSetContentUrl = "CreationSetContentUrl";
        public const string CreationSelectTag = "CreationSelectTag";
        public const string CreationInvalidUrl = "CreationInvalidUrl";
        public const string CreationProjectAlreadyExists = "CreationProjectAlreadyExists";
        public const string CreationCompleted = "CreationCompleted";
        
        public const string EditUserName = "EditUserName";
        public const string EditUserNameIsTooLong = "EditUserNameIsTooLong";
        
        public const string ProfileInfo = "ProfileInfo";
        
        public const string ProjectManagementSelectAction = "ProjectManagementSelectAction";
        public const string ProjectManagementSelectProject = "ProjectManagementSelectProject";
        public const string ProjectManagementStat = "ProjectManagementStat";
        public const string ProjectManagementDeleted = "ProjectManagementDeleted";
        
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
        
        public const string CatalogFilterTagsAreSelected = "CatalogFilterTagsAreSelected";
        
        public const string CatalogMoveNext = "CatalogMoveNext";
        public const string CatalogMovePrev = "CatalogMovePrev";
        public const string CatalogLike = "CatalogLike";
        public const string CatalogDislike = "CatalogDislike";
        public const string CatalogComplaint = "CatalogComplaint";
        public const string CatalogDownload = "CatalogDownload";
        
        public const string ProjectViewStatistics = "ProjectViewStatistics";
        public const string ProjectDelete = "ProjectDelete";
    }
}
