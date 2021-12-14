using Executor.Common.Models.Settings;
using Executor.Domain.Models;
using Executor.Infrastructure.Persistence.Common;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Executor.Infrastructure.Persistence;

public class ProjectsRepo : IRepo<string, Project>
{
    private readonly IMongoCollection<Project> _collection;

    public ProjectsRepo(
        IOptions<ProjectsDatabaseSettings> settings)
    {
        var mongoClient = new MongoClient(
            settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            settings.Value.DatabaseName);

        _collection = mongoDatabase.GetCollection<Project>(
            settings.Value.ProjectsCollectionName);
    }
    
    public async Task<IEnumerable<Project>> GetAsync(
        CancellationToken cancellationToken = default) =>
        await _collection.Find(_ => true)
            .ToListAsync(cancellationToken);

    public Task<Project> GetAsync(
        string key, 
        CancellationToken cancellationToken = default) =>
        _collection.Find(x => x.Id == key)
            .FirstOrDefaultAsync(cancellationToken);

    public Task CreateAsync(
        Project entity, 
        CancellationToken cancellationToken = default) =>
        _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

    public Task UpdateAsync(
        string key, Project entity, 
        CancellationToken cancellationToken = default) =>
        _collection.ReplaceOneAsync(
            x => x.Id == key, 
            entity, 
            cancellationToken: cancellationToken);

    public Task DeleteAsync(
        string key, 
        CancellationToken cancellationToken = default) =>
        _collection.DeleteOneAsync(x => x.Id == key, cancellationToken);
}