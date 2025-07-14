using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class EditUserDataState : UserState
{
    public UserDataAttribute AttributeToEdit { get; set; }
    
    public override void Reset()
    {
        AttributeToEdit = default;
    }
}
