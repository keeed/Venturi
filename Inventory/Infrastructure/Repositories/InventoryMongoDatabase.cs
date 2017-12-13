using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class InventoryMongoDatabase
    {
        private readonly IMongoDatabase _mongoDatabase;

        public InventoryMongoDatabase(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        public IMongoClient Client => _mongoDatabase.Client;

        public DatabaseNamespace DatabaseNamespace => _mongoDatabase.DatabaseNamespace;

        public MongoDatabaseSettings Settings => _mongoDatabase.Settings;

        public void CreateCollection(string name, CreateCollectionOptions options = null, CancellationToken cancellationToken = default (CancellationToken))
        {
            _mongoDatabase.CreateCollection(name, options, cancellationToken);
        }

        public Task CreateCollectionAsync(string name, CreateCollectionOptions options = null, CancellationToken cancellationToken = default (CancellationToken))
        {
            return _mongoDatabase.CreateCollectionAsync(name, options, cancellationToken);
        }

        public void CreateView<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default (CancellationToken))
        {
            _mongoDatabase.CreateView(viewName, viewOn, pipeline, options, cancellationToken);
        }

        public Task CreateViewAsync<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default (CancellationToken))
        {
            return _mongoDatabase.CreateViewAsync(viewName, viewOn, pipeline, options, cancellationToken);
        }

        public void DropCollection(string name, CancellationToken cancellationToken = default (CancellationToken))
        {
            _mongoDatabase.DropCollection(name, cancellationToken);
        }

        public Task DropCollectionAsync (string name, CancellationToken cancellationToken = default (CancellationToken))
        {
            return _mongoDatabase.DropCollectionAsync(name, cancellationToken);
        }

        public IMongoCollection<TDocument> GetCollection<TDocument>(string name, MongoCollectionSettings settings = null)
        {
            return _mongoDatabase.GetCollection<TDocument>(name, settings);
        }

        public IAsyncCursor<BsonDocument> ListCollections(ListCollectionsOptions options = null, CancellationToken cancellationToken = default (CancellationToken))
        {
            return _mongoDatabase.ListCollections(options, cancellationToken);
        }

        public Task<IAsyncCursor<BsonDocument>> ListCollectionsAsync (ListCollectionsOptions options = null, CancellationToken cancellationToken = default (CancellationToken))
        {
            return _mongoDatabase.ListCollectionsAsync(options, cancellationToken);
        }

        public void RenameCollection(string oldName, string newName, RenameCollectionOptions options = null, CancellationToken cancellationToken = default (CancellationToken))
        {
            _mongoDatabase.RenameCollection(oldName, newName, options, cancellationToken);
        }

        public Task RenameCollectionAsync(string oldName, string newName, RenameCollectionOptions options = null, CancellationToken cancellationToken = default (CancellationToken))
        {
            return _mongoDatabase.RenameCollectionAsync(oldName, newName, options, cancellationToken);
        }

        public TResult RunCommand<TResult>(Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default (CancellationToken))
        {
            return _mongoDatabase.RunCommand<TResult>(command, readPreference, cancellationToken);
        }

        public Task<TResult> RunCommandAsync<TResult>(Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default (CancellationToken))
        {
            return _mongoDatabase.RunCommandAsync<TResult>(command, readPreference, cancellationToken);
        }

        public IMongoDatabase WithReadConcern(ReadConcern readConcern)
        {
            return _mongoDatabase.WithReadConcern(readConcern);
        }

        public IMongoDatabase WithReadPreference(ReadPreference readPreference)
        {
            return _mongoDatabase.WithReadPreference(readPreference);
        }

        public IMongoDatabase WithWriteConcern(WriteConcern writeConcern)
        {
            return _mongoDatabase.WithWriteConcern(writeConcern);
        }
    }
}