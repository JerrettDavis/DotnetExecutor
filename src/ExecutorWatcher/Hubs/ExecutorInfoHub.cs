using Microsoft.AspNetCore.SignalR;

namespace ExecutorWatcher.Hubs;

public class ExecutorInfoHub : Hub<IExecutorInfoClient>
{
}