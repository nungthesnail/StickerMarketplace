namespace Marketplace.Core.Models.Enums.UserStates;

public enum UserCreationProgress
{
    SetName,
    Completed
}

public static class UserCreationProgressExtensions
{
    public static UserCreationProgress GetNext(this UserCreationProgress progress)
    {
        return progress switch
        {
            UserCreationProgress.SetName => UserCreationProgress.Completed,
            UserCreationProgress.Completed => UserCreationProgress.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(progress), progress, null)
        };
    }

    public static UserCreationProgress GetPrevious(this UserCreationProgress progress)
    {
        return progress switch
        {
            UserCreationProgress.Completed => UserCreationProgress.SetName,
            UserCreationProgress.SetName => UserCreationProgress.SetName,
            _ => throw new ArgumentOutOfRangeException(nameof(progress), progress, null)
        };
    }
}

