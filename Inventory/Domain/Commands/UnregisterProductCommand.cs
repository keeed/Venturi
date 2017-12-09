using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class UnregisterProductCommand : Command
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }

        public UnregisterProductCommand(Guid inventoryId, Guid productId)
        {
            InventoryId = inventoryId;
            ProductId = productId;
        }
    }

    public class UnregisterProductCommandHandler : ICommandAsyncHandler<UnregisterProductCommand>
    {
        private readonly IRepository<Inventory, InventoryId> _inventoryRepository;

        public UnregisterProductCommandHandler(IRepository<Inventory, InventoryId> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task HandleAsync(UnregisterProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Inventory inventory = await _inventoryRepository.GetByIdAsync(new InventoryId(command.InventoryId), cancellationToken).ConfigureAwait(false);
            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory not found.");
            }

            inventory.UnregisterProduct(new ProductId(command.ProductId));

            await _inventoryRepository.SaveAsync(inventory, cancellationToken).ConfigureAwait(false);
        }
    }
}