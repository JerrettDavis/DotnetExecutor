namespace DotnetExecutor.Services;

public class ProjectFactory : IProjectFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public ProjectFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public ProjectFacade Create(
        string program)
    {
        var logger = _loggerFactory.CreateLogger<ProjectFacade>();
        return new(program, logger);
    }
}