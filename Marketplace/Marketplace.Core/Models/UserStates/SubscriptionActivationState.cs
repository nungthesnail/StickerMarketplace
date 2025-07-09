using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class SubscriptionActivationState : FormUserState<SubscriptionActivationProgress>
{
    public TransactionMethod? Method { get; set; }
    public int PriceIdx { get; set; }
    
    public override void Reset()
    {
        Method = null;
        Progress = default;
    }

    public override bool Completed => false; // The state finishing by the payments handler
    public override void MoveProgressNext() => Progress = Progress.GetNext();
    public override void MoveProgressBack() => Progress = Progress.GetPrevious();
    public override SubscriptionActivationProgress Progress { get; set; }

    public string MethodAsCurrency()
    {
        if (!Method.HasValue)
            throw new InvalidOperationException("Payment method is not specified");
        
        return Method switch
        {
            TransactionMethod.TelegramStars => "XTR",
            _ => throw new ArgumentOutOfRangeException(nameof(Method), Method, null)
        };
    }
}
