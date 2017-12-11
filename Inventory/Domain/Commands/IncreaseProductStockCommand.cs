using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class IncreaseProductStockCommand : Command
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }
        public int AmountToIncrease { get; }

        public IncreaseProductStockCommand(Guid productId, int amountToIncrease)
        {
            ProductId = productId;
            AmountToIncrease = amountToIncrease;
        }
    }

    public class IncreaseProductStockCommandHandler : ICommandAsyncHandler<IncreaseProductStockCommand>
    {
        private readonly IRepository<Product, ProductId> _productRepository;

        public IncreaseProductStockCommandHandler(IRepository<Product, ProductId> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(IncreaseProductStockCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetByIdAsync(new ProductId(command.ProductId), cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }
            
            product.IncreaseProductStock(command.AmountToIncrease);

            await _productRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        }
    }
}