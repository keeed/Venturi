using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;

namespace Domain.Repositories
{
    public class PublishingProductRepository : IProductRepository
    {
        private readonly IProductRepository _productRepository;
        private readonly IEventPublisher _eventPublisher;

        public PublishingProductRepository(IProductRepository baseRepository, IEventPublisher eventPublisher)
        {
            _productRepository = baseRepository;
            _eventPublisher = eventPublisher;
        }

        public Task<Product> GetProductByIdAsync(ProductId productId, CancellationToken ct = default(CancellationToken))
        {
            return _productRepository.GetProductByIdAsync(productId, ct);
        }

        public async Task SaveAsync(Product product, CancellationToken ct = default(CancellationToken))
        {
            IEventOriginator eventOriginator = product as IEventOriginator;
            IEnumerable<IEvent> eventsCopy = eventOriginator.GetEvents();

            await _productRepository.SaveAsync(product, ct).ConfigureAwait(false);

            if(eventsCopy.Any())
            {
                await _eventPublisher.PublishAsync(eventsCopy, ct).ConfigureAwait(false);
            }

            eventOriginator.ClearEvents();
        }
    }
}