using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.Repositories;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class InventoryMongoRepository : IInventoryRepository
    {
        private readonly IMongoCollection<InventoryMongoDocument> _inventoryCollection;

        public InventoryMongoRepository(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDatabase= mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _inventoryCollection = mongoDatabase.GetCollection<InventoryMongoDocument>(nameof(Inventory));
        }

        public async Task<Inventory> GetByIdAsync(InventoryId productId, CancellationToken ct = default(CancellationToken))
        {
            var filter = Builders<InventoryMongoDocument>.Filter.Eq(i => i.InventoryId, productId.Value);
            IAsyncCursor<InventoryMongoDocument> findResult = await _inventoryCollection.FindAsync(filter, null, ct);

            InventoryMongoDocument inventoryDoc = await findResult.FirstOrDefaultAsync(ct);
            if (inventoryDoc == null)
            {
                // No inventory found.
                return null;
            }
            
            // Translate.
            return new Inventory(new InventoryId(inventoryDoc.InventoryId), 
                                 new WarehouseId(inventoryDoc.WarehouseId), 
                                 inventoryDoc.Catalogs.Select(c => new Catalog(new InventoryId(c.InventoryId), c.CatalogName)).ToList());
        }

        public Task SaveAsync(Inventory inventory, CancellationToken ct = default(CancellationToken))
        {
            InventoryState state = inventory.GetCurrentState();

            var filter = Builders<InventoryMongoDocument>.Filter.Eq(i => i.InventoryId, inventory.Id.Value);

            var update = Builders<InventoryMongoDocument>.Update.Set(i => i.InventoryId, state.InventoryId.Value)
                                                                .Set(i => i.WarehouseId, state.WarehouseId.Value)
                                                                .Set(p => p.Catalogs, state.Catalogs.Select(c => new CatalogMongoDocument()
                                                                                                            {
                                                                                                                InventoryId = c.InventoryId.Value,
                                                                                                                CatalogName = c.Name
                                                                                                            }).ToList());

            // Upsert.
            return _inventoryCollection.FindOneAndUpdateAsync(filter, 
                                                              update, 
                                                              new FindOneAndUpdateOptions<InventoryMongoDocument, InventoryMongoDocument>() 
                                                              { 
                                                                  IsUpsert = true 
                                                              }, 
                                                              ct);
        }

        private class InventoryMongoDocument
        {
            [BsonId]
            public Guid InventoryId { get; set; }
            public Guid WarehouseId { get; set; }

            // public ICollection<Product> Products { get; set; }
            public ICollection<CatalogMongoDocument> Catalogs { get; set; } = new List<CatalogMongoDocument>();
        }

        private class CatalogMongoDocument
        {
            public Guid InventoryId { get; set; }
            public string CatalogName { get; set; }
        }
    }
}