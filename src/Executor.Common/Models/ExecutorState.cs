namespace Executor.Common.Models;

public enum ExecutorState
{
    Waiting,
    Provisioning,
    Running,
    ShuttingDown,
    Error
}