using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class MarkProductAsNotForSaleCommand : Command
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }

        public MarkProductAsNotForSaleCommand(Guid inventoryId, Guid productId)
        {
            InventoryId = inventoryId;
            ProductId = productId;
        }
    }

    public class MarkProductAsNotForSaleCommandHandler : ICommandAsyncHandler<MarkProductAsNotForSaleCommand>
    {
        private readonly IRepository<Inventory, InventoryId> _inventoryRepository;

        public MarkProductAsNotForSaleCommandHandler(IRepository<Inventory, InventoryId> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task HandleAsync(MarkProductAsNotForSaleCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Inventory inventory = await _inventoryRepository.GetByIdAsync(new InventoryId(command.InventoryId), cancellationToken).ConfigureAwait(false);
            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory not found.");
            }

            inventory.MarkProductAsNotForSale(new ProductId(command.ProductId));

            await _inventoryRepository.SaveAsync(inventory, cancellationToken).ConfigureAwait(false);
        }
    }
}