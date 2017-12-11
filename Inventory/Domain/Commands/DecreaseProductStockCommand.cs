using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class DecreaseProductStockCommand : Command
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }
        public int AmountToDecrease { get; }

        public DecreaseProductStockCommand(Guid inventoryId, Guid productId, int amountToDecrease)
        {
            InventoryId = inventoryId;
            ProductId = productId;
            AmountToDecrease = amountToDecrease;
        }
    }

    public class DecreaseProductStockCommandHandler : ICommandAsyncHandler<DecreaseProductStockCommand>
    {
        private readonly IRepository<Product, ProductId> _productRepository;

        public DecreaseProductStockCommandHandler(IRepository<Product, ProductId> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(DecreaseProductStockCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetByIdAsync(new ProductId(command.ProductId), cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }
            
            product.DecreaseProductStock(command.AmountToDecrease);

            await _productRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        }
    }
}