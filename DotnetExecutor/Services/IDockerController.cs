namespace DotnetExecutor.Services;

public interface IDockerController
{
    Task<string> RunAppInContainer(ProjectFacade project);
    event EventHandler<string>? LogEmitted;
}