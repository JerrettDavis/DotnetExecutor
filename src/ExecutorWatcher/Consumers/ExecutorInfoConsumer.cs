using DotnetExecutor.Models;
using Executor.Common.Models;
using ExecutorWatcher.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace ExecutorWatcher.Consumers;

public class ExecutorInfoConsumer : IConsumer<ExecutorInfo>
{
    private readonly IHubContext<ExecutorInfoHub, IExecutorInfoClient> _infoHub;

    public ExecutorInfoConsumer(
        IHubContext<ExecutorInfoHub,IExecutorInfoClient> infoHub)
    {
        _infoHub = infoHub;
    }

    public Task Consume(ConsumeContext<ExecutorInfo> context)
    {
        return _infoHub.Clients.All.ExecutorInfoReceived(context.Message);
    }
}