namespace Executor.Domain.Models;

public class Project
{
    public string Id { get; set; } = null!;
    public IEnumerable<NugetPackage> NugetPackages { get; set; } = new HashSet<NugetPackage>();
    public IEnumerable<ProjectFile> ProjectFiles { get; set; } = new HashSet<ProjectFile>();
    public string? Output { get; set; }
}