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
            var filter = Builders<InventoryMongoDocument>.Filter.Eq(i => i.Id, productId.Value);
            IAsyncCursor<InventoryMongoDocument> findResult = await _inventoryCollection.FindAsync(filter, null, ct);

            InventoryMongoDocument document = await findResult.FirstOrDefaultAsync(ct);
            if (document == null)
            {
                // No inventory found.
                return null;
            }
            
            // Translate.
            return new Inventory(new InventoryId(document.Id), new WarehouseId(document.WarehouseId), document.Products, document.Catalogs);
        }

        public Task SaveAsync(Inventory inventory, CancellationToken ct = default(CancellationToken))
        {
            var filter = Builders<InventoryMongoDocument>.Filter.Eq(i => i.Id, inventory.Id.Value);

            var update = Builders<InventoryMongoDocument>.Update.Set(i => i.Id, inventory.Id.Value)
                                                                .Set(i => i.WarehouseId, inventory.WarehouseId.Value)
                                                                .Set(i => i.Products, inventory.Products.ToList())
                                                                .Set(i => i.Catalogs, inventory.Catalogs.ToList());

            // Upsert.
            return _inventoryCollection.FindOneAndUpdateAsync(filter, 
                                                              update, 
                                                              new FindOneAndUpdateOptions<InventoryMongoDocument, InventoryMongoDocument>() 
                                                              { 
                                                                  IsUpsert = true 
                                                              }, 
                                                              ct);
        }
    }

    internal class InventoryMongoDocument
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid WarehouseId { get; set; }

        public ICollection<Product> Products { get; set; }
        public ICollection<Catalog> Catalogs { get; set; }

        public InventoryMongoDocument(Guid id, Guid warehouseId, IEnumerable<Product> products, IEnumerable<Catalog> catalogs)
        {
            Id = id;
            WarehouseId = warehouseId;
            Products = products.ToList();
            Catalogs = catalogs.ToList();
        }
    }
}