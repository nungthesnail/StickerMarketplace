namespace Marketplace.Core.Models.Enums.UserStates;

public enum ProjectCreationProgress
{
    SetCategory,
    SetName,
    SetDescription,
    SetImageId,
    SetContentUrl,
    SetProjectTag,
    Completed
}

public static class ProjectCreationProgressExtensions
{
    public static ProjectCreationProgress GetNext(this ProjectCreationProgress progress)
    {
        return progress switch
        {
            ProjectCreationProgress.SetCategory => ProjectCreationProgress.SetName,
            ProjectCreationProgress.SetName => ProjectCreationProgress.SetDescription,
            ProjectCreationProgress.SetDescription => ProjectCreationProgress.SetImageId,
            ProjectCreationProgress.SetImageId => ProjectCreationProgress.SetContentUrl,
            ProjectCreationProgress.SetContentUrl => ProjectCreationProgress.SetProjectTag,
            ProjectCreationProgress.SetProjectTag => ProjectCreationProgress.Completed,
            ProjectCreationProgress.Completed => ProjectCreationProgress.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(progress), progress, null)
        };
    }
    
    public static ProjectCreationProgress GetPrevious(this ProjectCreationProgress progress)
    {

        return progress switch
        {
            ProjectCreationProgress.Completed => ProjectCreationProgress.SetProjectTag,
            ProjectCreationProgress.SetProjectTag => ProjectCreationProgress.SetContentUrl,
            ProjectCreationProgress.SetContentUrl => ProjectCreationProgress.SetImageId,
            ProjectCreationProgress.SetImageId => ProjectCreationProgress.SetDescription,
            ProjectCreationProgress.SetDescription => ProjectCreationProgress.SetName,
            ProjectCreationProgress.SetName => ProjectCreationProgress.SetCategory,
            ProjectCreationProgress.SetCategory => ProjectCreationProgress.SetCategory,
            _ => throw new ArgumentOutOfRangeException(nameof(progress), progress, null)
        };
    }
}
