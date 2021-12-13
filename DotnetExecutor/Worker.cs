using DotnetExecutor.Models;
using DotnetExecutor.Services;
using Executor.Common.Models;
using MassTransit;

namespace DotnetExecutor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBus _bus;
    private readonly IProjectFactory _projectFactory;
    private readonly IDockerController _dockerController;
    public Guid _id;

    public Worker(
        ILogger<Worker> logger,
        IBus bus,
        IProjectFactory projectFactory, 
        IDockerController dockerController)
    {
        _logger = logger;
        _bus = bus;
        _projectFactory = projectFactory;
        _dockerController = dockerController;
        _id = Guid.NewGuid();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
            await _bus.Publish(new ExecutorInfo
            {
                Executor = _id,
                State = ExecutorState.Waiting
            }, stoppingToken);

            const string code = "Console.WriteLine(\"Hello, World!\");";
            await using var project = _projectFactory.Create(code);
            
            await project.CreateProject();
// var (output, errors) = await proj.RunProject();
            _dockerController.LogEmitted +=
                (_,log) => _logger.LogDebug("Log Received {Log}", log);
            var output = await _dockerController.RunAppInContainer(project, stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }
}