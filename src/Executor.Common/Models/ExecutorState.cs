namespace DotnetExecutor.Models;

public enum ExecutorState
{
    Waiting,
    Provisioning,
    Running,
    ShuttingDown,
    Error
}