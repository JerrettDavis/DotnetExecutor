using DotnetExecutor.Services;
using Executor.Common.Models;
using MassTransit;

namespace DotnetExecutor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBus _bus;
    public readonly Guid _id;

    public Worker(
        ILogger<Worker> logger,
        IBus bus)
    {
        _logger = logger;
        _bus = bus;
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

            await Task.Delay(1000, stoppingToken);
        }
    }
}