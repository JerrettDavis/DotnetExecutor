using Executor.Common.Models;
using Executor.Common.Models.Settings;
using Executor.Domain.Models;
using Executor.Infrastructure.Persistence;
using Executor.Infrastructure.Persistence.Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddMassTransitHostedService(true);

builder.Services.Configure<ProjectsDatabaseSettings>(
    builder.Configuration.GetSection("ProjectsDatabase"));

builder.Services
    .AddSingleton<IRepo<string,Project>, ProjectsRepo>();

builder.Services
    .AddSwaggerDocument()
    .AddEndpointsApiExplorer();

var app = builder.Build();

app.UseOpenApi();
app.UseSwaggerUi3();
app.UseReDoc();

app.MapGet("/", ctx =>
{
    ctx.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.MapGet("/Projects", () => "Hello World!");
app.MapPost("/Projects", async (
        [FromServices] IRepo<string,Project> projectsService
        , Project project
        , CancellationToken cancellationToken) =>
{
    await projectsService.CreateAsync(project, cancellationToken);
});
app.MapPut("/Projects", async (
        [FromServices] IRepo<string,Project> projectsService
        , string id
        , Project project
        , CancellationToken cancellationToken) =>
    await projectsService.UpdateAsync(id, project, cancellationToken));
app.MapPost("/Projects/{id}/Run", async (
    [FromServices] IBus bus
    , string id) =>
{
    var endpoint = await bus.GetSendEndpoint(new Uri("queue:run-project"));
    await endpoint.Send<RunProject>(new() {Id = id});
});

app.Run();