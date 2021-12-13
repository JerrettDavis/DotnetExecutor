using DotnetExecutor;
using DotnetExecutor.Services;
using MassTransit;
using Serilog;
using Serilog.Events;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
    
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices(services =>
        {
            services.AddSingleton<IProjectFactory, ProjectFactory>();
            services.AddSingleton<IDockerController, DockerController>();
         
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) => cfg.ConfigureEndpoints(context));
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



