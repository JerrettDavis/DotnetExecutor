using Executor.Common.Models;

namespace ExecutorWatcher.Hubs;

public interface IExecutorInfoClient
{
    Task ExecutorInfoReceived(ExecutorInfo info);
}