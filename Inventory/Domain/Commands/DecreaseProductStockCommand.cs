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
        private readonly IRepository<Inventory, InventoryId> _inventoryRepository;

        public DecreaseProductStockCommandHandler(IRepository<Inventory, InventoryId> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task HandleAsync(DecreaseProductStockCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Inventory inventory = await _inventoryRepository.GetByIdAsync(new InventoryId(command.InventoryId), cancellationToken).ConfigureAwait(false);
            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory not found.");
            }
            
            inventory.DecreaseProductStock(new ProductId(command.ProductId), command.AmountToDecrease);

            await _inventoryRepository.SaveAsync(inventory, cancellationToken).ConfigureAwait(false);
        }
    }
}