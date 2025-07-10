namespace Marketplace.Core.Models.UserStates;

public class DefaultUserState : UserState
{
    public override void Reset()
    { }

    public static DefaultUserState Create() => new();
}
