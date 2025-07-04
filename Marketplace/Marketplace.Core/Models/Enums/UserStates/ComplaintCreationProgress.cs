namespace Marketplace.Core.Models.Enums.UserStates;

public enum ComplaintCreationProgress
{
    SelectTopic,
    SetContent,
    Completed
}

public static class ComplaintCreationProgressExtensions
{
    public static ComplaintCreationProgress GetNext(this ComplaintCreationProgress progress)
    {
        return progress switch
        {
            ComplaintCreationProgress.SelectTopic => ComplaintCreationProgress.SetContent,
            ComplaintCreationProgress.SetContent => ComplaintCreationProgress.Completed,
            ComplaintCreationProgress.Completed => ComplaintCreationProgress.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(progress), progress, null)
        };
    }

    public static ComplaintCreationProgress GetPrevious(this ComplaintCreationProgress progress)
    {
        return progress switch
        {
            ComplaintCreationProgress.Completed => ComplaintCreationProgress.SetContent,
            ComplaintCreationProgress.SetContent => ComplaintCreationProgress.SelectTopic,
            ComplaintCreationProgress.SelectTopic => ComplaintCreationProgress.SelectTopic,
            _ => throw new ArgumentOutOfRangeException(nameof(progress), progress, null)
        };
    }
}
