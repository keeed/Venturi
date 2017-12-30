using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class RepriceProductCommand : Command
    {
        public Guid ProductId { get; }
        public decimal NewPrice { get; }

        public RepriceProductCommand(Guid productId, decimal newPrice)
        {
            ProductId = productId;
            NewPrice = newPrice;
        }
    }

    public class RepriceProductCommandHandler : ICommandAsyncHandler<RepriceProductCommand>
    {
        private readonly IProductRepository _productRepository;

        public RepriceProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(RepriceProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetProductByIdAsync(new ProductId(command.ProductId), cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            product.Reprice(command.NewPrice);

            await _productRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        }
    }
}