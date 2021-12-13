using System.Text;
using CliWrap;
using CliWrap.Buffered;

namespace DotnetExecutor.Services;

public class ProjectFacade : IDisposable, IAsyncDisposable
{
    private readonly ILogger<ProjectFacade> _logger;
    public Guid Id { get; }

    public ProjectFacade(
        string project,
        ILogger<ProjectFacade> logger) : 
        this(project, Array.Empty<KeyValuePair<string, string>>(), logger)
    {
    }
    
    public ProjectFacade(
        string program,
        IEnumerable<KeyValuePair<string, string>> nugetPackages,
        ILogger<ProjectFacade> logger) 
    {
        NugetPackages = nugetPackages;
        Program = program;
        Id = Guid.NewGuid();
        
        _logger = logger;
        logger.LogInformation("Project Facade '{Id}' Created", Id);
    }

    public IEnumerable<KeyValuePair<string, string>> NugetPackages { get; }
    public string Program { get; }
    public string Location => Path.Combine(
        Directory.GetCurrentDirectory(), "Projects", Id.ToString());

    public async Task CreateProject()
    {
        await CreateDirectory();
        _logger.LogInformation("Using dotnet CLI to create {Id} project", Id);
        await Cli.Wrap("dotnet")
            .WithArguments("new console")
            .WithWorkingDirectory(Location)
            .ExecuteAsync();
        
        _logger.LogDebug("Writing {Id} program to disk", Id);
        var program = Path.Combine(Location, "Program.cs");
        await File.WriteAllTextAsync(program, Program);
    }

    private Task DestroyProject()
    {
        _logger.LogInformation("Deleting {Id}", Id);
        return Task.Run(() => DeleteDirectory(Location));
    }
    
    public static void DeleteDirectory(string target, bool recursive = true)
    {
        var tfilename = Path.GetDirectoryName(target) +
                           (target.Contains(Path.DirectorySeparatorChar.ToString()) ? Path.DirectorySeparatorChar.ToString() : string.Empty) +
                           Path.GetRandomFileName();
        Directory.Move(target, tfilename);
        Directory.Delete(tfilename, recursive);
    }
    
    private async Task CreateDirectory()
    {
        _logger.LogDebug("Creating Directory if it does not exist");
        await Task.Run(() => Directory.CreateDirectory(Location));
    }

    public void Dispose()
    {
        DestroyProject().RunSynchronously();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DestroyProject();
        GC.SuppressFinalize(this);
    }
}