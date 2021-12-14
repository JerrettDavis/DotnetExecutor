namespace Executor.Common.Models;

public record ExecutorInfo
{
    public Guid Executor { get; init; }
    public ExecutorState State { get; init; }
}