using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class IncreaseProductStockCommand : Command
    {
        public Guid ProductId { get; }
        public int Quantity { get; }

        public IncreaseProductStockCommand(Guid productId, int amountToIncrease)
        {
            ProductId = productId;
            Quantity = amountToIncrease;
        }
    }

    public class IncreaseProductStockCommandHandler : ICommandAsyncHandler<IncreaseProductStockCommand>
    {
        private readonly IProductRepository _productRepository;

        public IncreaseProductStockCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(IncreaseProductStockCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetProductByIdAsync(new ProductId(command.ProductId), cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }
            
            product.IncreaseProductStock(command.Quantity);

            await _productRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        }
    }
}