namespace Executor.Common.Models.Settings;

public class ProjectsDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string ProjectsCollectionName { get; set; } = null!;
}