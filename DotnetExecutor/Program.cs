using DotnetExecutor;
using DotnetExecutor.Consumers;
using DotnetExecutor.Services;
using Executor.Common.Models.Settings;
using Executor.Domain.Models;
using Executor.Infrastructure.Persistence;
using Executor.Infrastructure.Persistence.Common;
using MassTransit;
using Serilog;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
    
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((builder,services) =>
        {
            services.AddSingleton<IProjectFactory, ProjectFactory>();
            services.AddSingleton<IDockerController, DockerController>();
            
            services
                .AddSingleton<IRepo<string,Project>, ProjectsRepo>();
            
            services.Configure<ProjectsDatabaseSettings>(
                builder.Configuration.GetSection("ProjectsDatabase"));
         
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<RunProjectConsumer>(typeof(RunProjectConsumerDefinition));

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
            services.AddMassTransitHostedService(true);
            
            services.AddHostedService<Worker>();
        })
        .Build();
    
    Log.Information("Starting web host");
    
    await host.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();   
}



