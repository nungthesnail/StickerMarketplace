namespace Marketplace.Core.Models.UserStates;

public interface IFormUserState<TProgress> : IUserState
    where TProgress : struct
{
    bool Completed { get; }
    void MoveProgressNext();
    void MoveProgressBack();
    TProgress Progress { get; }
}
