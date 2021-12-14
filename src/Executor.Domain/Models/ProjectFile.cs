namespace Executor.Domain.Models;

public class ProjectFile
{
    public string FileName { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Contents { get; set; } = null!;
}