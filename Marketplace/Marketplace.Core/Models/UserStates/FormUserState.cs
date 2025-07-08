namespace Marketplace.Core.Models.UserStates;

public abstract class FormUserState<TProgress> : UserState
    where TProgress : struct
{
    public abstract bool Completed { get; }
    public abstract void MoveProgressNext();
    public abstract void MoveProgressBack();
    public abstract TProgress Progress { get; set; }
}
