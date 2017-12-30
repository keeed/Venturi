using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class DecreaseProductStockCommand : Command
    {
        public Guid ProductId { get; }
        public int Quantity { get; }

        public DecreaseProductStockCommand(Guid productId, int amountToDecrease)
        {
            ProductId = productId;
            Quantity = amountToDecrease;
        }
    }

    public class DecreaseProductStockCommandHandler : ICommandAsyncHandler<DecreaseProductStockCommand>
    {
        private readonly IProductRepository _productRepository;

        public DecreaseProductStockCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(DecreaseProductStockCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetProductByIdAsync(new ProductId(command.ProductId), cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }
            
            product.DecreaseProductStock(command.Quantity);

            await _productRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        }
    }
}