namespace DotnetExecutor.Services;

public interface IDockerController
{
    Task<string> RunAppInContainer(ProjectFacade project,
        CancellationToken cancellationToken = default);
    event EventHandler<string>? LogEmitted;
}