using System.Text;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Commands;
using Ductus.FluentDocker.Extensions;
using Ductus.FluentDocker.Model.Builders;
using Ductus.FluentDocker.Services;

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

    public Task<string> RunAppInContainer(ProjectFacade project)
    {
        return Task.Run(() =>
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
            
            var hosts = new Hosts().Discover();
            var docker = hosts.FirstOrDefault(x => x.IsNative) ?? 
                         hosts.FirstOrDefault(x => x.Name == "default")!;
            using var logs = docker.Host.Logs(project.Id.ToString());
            var logBuilder = new StringBuilder();
            while (!logs.IsFinished)
            {
                var line = logs.TryRead(5000); // Do a read with timeout
                if (null == line)
                    break;

                logBuilder.Append(line);
                OnLogEmitted(line);
            }
            
            while (container.GetConfiguration(true).State.ToServiceState() == 
                   ServiceRunningState.Running)
            {
                _logger.LogDebug("Waiting for container {Id} to complete", project.Id);
                Thread.Sleep(100);
            }

            var output = string.Join(Environment.NewLine, logBuilder.ToString());

            return string.Join(Environment.NewLine, output);
        });
    }
}