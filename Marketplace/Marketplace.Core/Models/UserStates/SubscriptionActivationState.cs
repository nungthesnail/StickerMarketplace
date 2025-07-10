using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class SubscriptionActivationState : FormUserState<SubscriptionActivationProgress>
{
    public SubscriptionRenewMethod? RenewMethod { get; set; }
    public int? PriceIdx { get; set; }
    
    public override void Reset()
    {
        RenewMethod = null;
        PriceIdx = null;
        Progress = default;
    }

    public override bool Completed => false; // The state finishing by the payments handler
    public override void MoveProgressNext() => Progress = Progress.GetNext();
    public override void MoveProgressBack() => Progress = Progress.GetPrevious();
    public override SubscriptionActivationProgress Progress { get; set; }

    public string MethodAsCurrency()
    {
        if (!RenewMethod.HasValue)
            throw new InvalidOperationException("Payment method is not specified");
        
        return RenewMethod switch
        {
            SubscriptionRenewMethod.TelegramStars => "XTR",
            _ => throw new ArgumentOutOfRangeException(nameof(RenewMethod), RenewMethod, null)
        };
    }
}
