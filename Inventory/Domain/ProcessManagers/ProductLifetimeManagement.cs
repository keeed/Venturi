using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents;
using Domain.Repositories;
using Xer.Cqrs.EventStack;

namespace Domain.ProcessManagers
{
    public class ProductLifetimeManagement : IEventAsyncHandler<ProductRegisteredEvent>,
                                             IEventAsyncHandler<ProductUnregisteredEvent>,
                                             IEventAsyncHandler<ProductAddedToCatalogEvent>,
                                             IEventAsyncHandler<ProductRemovedFromCatalogEvent>
    {
        private readonly IRepository<Product, ProductId> _productRepository;

        public ProductLifetimeManagement(IRepository<Product, ProductId> productRepository)
        {
            _productRepository = productRepository;
        }
        
        public Task HandleAsync(ProductRegisteredEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            ProductId productId = new ProductId(@event.ProductId);
            return _productRepository.SaveAsync(new Product(productId,
                                                            new InventoryId(@event.InventoryId),
                                                            @event.ProductName,
                                                            @event.ProductDescription,
                                                            new Price(@event.ProductPrice),
                                                            new Stock(productId, 0),
                                                            Enumerable.Empty<Catalog>(),
                                                            false));
        }

        public async Task HandleAsync(ProductUnregisteredEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetByIdAsync(new ProductId(@event.ProductId), cancellationToken).ConfigureAwait(false);
            if (product != null)
            {
                product.MarkAsUnregistered();
            }

            await _productRepository.SaveAsync(product).ConfigureAwait(false);
        }

        public async Task HandleAsync(ProductAddedToCatalogEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetByIdAsync(new ProductId(@event.ProductId), cancellationToken).ConfigureAwait(false);
            if (product != null)
            {
                product.AddProductToCatalog(new Catalog(new InventoryId(@event.InventoryId), @event.CatalogName));
            }

            await _productRepository.SaveAsync(product).ConfigureAwait(false);
        }

        public async Task HandleAsync(ProductRemovedFromCatalogEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetByIdAsync(new ProductId(@event.ProductId), cancellationToken).ConfigureAwait(false);
            if (product != null)
            {
                product.RemoveFromCatalog(new Catalog(new InventoryId(@event.InventoryId), @event.CatalogName));
            }

            await _productRepository.SaveAsync(product).ConfigureAwait(false);
        }
    }
}