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
        private readonly IRepository<Inventory, InventoryId> _inventoryRepository;

        public IncreaseProductStockCommandHandler(IRepository<Inventory, InventoryId> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task HandleAsync(IncreaseProductStockCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Inventory inventory = await _inventoryRepository.GetByIdAsync(new InventoryId(command.InventoryId), cancellationToken).ConfigureAwait(false);
            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory not found.");
            }
            
            inventory.IncreaseProductStock(new ProductId(command.ProductId), command.AmountToIncrease);

            await _inventoryRepository.SaveAsync(inventory, cancellationToken).ConfigureAwait(false);
        }
    }
}