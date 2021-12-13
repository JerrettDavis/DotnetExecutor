using System.Text;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Commands;
using Ductus.FluentDocker.Common;
using Ductus.FluentDocker.Extensions;
using Ductus.FluentDocker.Model.Builders;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;
using MassTransit.Topology.Observers;

namespace DotnetExecutor.Services;

public class DockerController : IDockerController
{
    private readonly ILogger<DockerController> _logger;
    public event EventHandler<string>? LogEmitted;

    public DockerController(ILogger<DockerController> logger)
    {
        _logger = logger;
    }

    protected virtual void OnLogEmitted(string log)
    {
        LogEmitted?.Invoke(this, log);
    }

    public Task<string> RunAppInContainer(
        ProjectFacade project,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            _logger.LogInformation("Setting up container {Id}", project.Id);
            using var container =
                new Builder().UseContainer()
                    .UseImage("mcr.microsoft.com/dotnet/sdk:6.0")
                    .RemoveVolumesOnDispose()
                    .WithName(project.Id.ToString())
                    .Mount(project.Location, "/app", MountType.ReadWrite)
                    .UseWorkDir("/app")
                    .Command("dotnet run -v n")
                    .Build()
                    .Start();
            
            _logger.LogDebug("Reading logs for container {Id}", project.Id);
            var result = await GetLogs(container, cancellationToken);

            return string.Join(Environment.NewLine, result);
        }, cancellationToken);
    }

    public Task<string> GetLogs(IContainerService container, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var logBuilder = new StringBuilder();
            using var logs = container.Logs(true);
            while (!logs.IsFinished)
            {

                var line = logs.TryRead(5000);
                if (null == line)
                    break;

                logBuilder.AppendLine(line);
                OnLogEmitted(line);
            }
                
            container.Dispose();

            return logBuilder.ToString();
        }, cancellationToken);
    }
}