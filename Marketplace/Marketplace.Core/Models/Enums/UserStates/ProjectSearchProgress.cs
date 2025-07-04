namespace Marketplace.Core.Models.Enums.UserStates;

public enum ProjectSearchProgress
{
    SelectCategory,
    SelectTags,
    Completed
}

public static class ProjectSearchProgressExtensions
{
    public static ProjectSearchProgress GetNext(this ProjectSearchProgress progress)
    {
        return progress switch
        {
            ProjectSearchProgress.SelectCategory => ProjectSearchProgress.SelectTags,
            ProjectSearchProgress.SelectTags => ProjectSearchProgress.Completed,
            ProjectSearchProgress.Completed => ProjectSearchProgress.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(progress), progress, null)
        };
    }

    public static ProjectSearchProgress GetPrevious(this ProjectSearchProgress progress)
    {
        return progress switch
        {
            ProjectSearchProgress.Completed => ProjectSearchProgress.SelectTags,
            ProjectSearchProgress.SelectTags => ProjectSearchProgress.SelectCategory,
            ProjectSearchProgress.SelectCategory => ProjectSearchProgress.SelectCategory,
            _ => throw new ArgumentOutOfRangeException(nameof(progress), progress, null)
        };
    }
}
