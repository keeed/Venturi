using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class UnregisterProductCommand : Command
    {
        public Guid ProductId { get; }

        public UnregisterProductCommand(Guid productId)
        {
            ProductId = productId;
        }
    }

    public class UnregisterProductCommandHandler : ICommandAsyncHandler<UnregisterProductCommand>
    {
        private readonly IRepository<Product, ProductId> _productRepository;

        public UnregisterProductCommandHandler(IRepository<Product, ProductId> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(UnregisterProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetByIdAsync(new ProductId(command.ProductId), cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            product.Unregister();

            await _productRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        }
    }
}