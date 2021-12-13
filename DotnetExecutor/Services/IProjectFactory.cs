namespace DotnetExecutor.Services;

public interface IProjectFactory
{
    ProjectFacade Create(
        string program);
}