using DotnetExecutor.Services;
using Executor.Common.Models;
using Executor.Domain.Models;
using Executor.Infrastructure.Persistence.Common;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;

namespace DotnetExecutor.Consumers;

public class RunProjectConsumer : IConsumer<RunProject>
{
    private readonly IProjectFactory _projectFactory;
    private readonly IDockerController _dockerController;
    private readonly IRepo<string, Project> _repo;
    private readonly ILogger<RunProjectConsumer> _logger;

    public RunProjectConsumer(
        IDockerController dockerController, 
        IProjectFactory projectFactory, 
        ILogger<RunProjectConsumer> logger, 
        IRepo<string, Project> repo)
    {
        _dockerController = dockerController;
        _projectFactory = projectFactory;
        _logger = logger;
        _repo = repo;
    }

    public async Task Consume(ConsumeContext<RunProject> context)
    {
        var projectId = context.Message.Id;
        _logger.LogInformation("Received run request for Project {Id}", projectId);
        
        var entity = await _repo.GetAsync(projectId, context.CancellationToken);
        const string code = "Console.WriteLine(\"Hello, World!\");";
        var source = entity.ProjectFiles.First().Contents;
        await using var project = _projectFactory.Create(source);
        
        await project.CreateProject();
        
        var output = await _dockerController.RunAppInContainer(project, context.CancellationToken);
        
        _logger.LogInformation("Project output: {Output}", output);
        await context.RespondAsync<RunProjectResult>(new(output));
    }
}

public class RunProjectConsumerDefinition :
    ConsumerDefinition<RunProjectConsumer>
{
    public RunProjectConsumerDefinition()
    {
        EndpointName = "run-project";
        ConcurrentMessageLimit = 8;
    }
    
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<RunProjectConsumer> consumerConfigurator)
    {
        // configure message retry with millisecond intervals
        endpointConfigurator.UseMessageRetry(r => r.Intervals(100,200,500,800,1000));

        // use the outbox to prevent duplicate events from being published
        endpointConfigurator.UseInMemoryOutbox();
    }
}