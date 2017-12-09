using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class RepriceProductCommand : Command
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }
        public decimal NewPrice { get; }

        public RepriceProductCommand(Guid inventoryId, Guid productId, decimal newPrice)
        {
            InventoryId = inventoryId;
            ProductId = productId;
            NewPrice = newPrice;
        }
    }

    public class RepriceProductCommandHandler : ICommandAsyncHandler<RepriceProductCommand>
    {
        private readonly IRepository<Inventory, InventoryId> _inventoryRepository;
        
        public RepriceProductCommandHandler(IRepository<Inventory, InventoryId> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task HandleAsync(RepriceProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Inventory inventory = await _inventoryRepository.GetByIdAsync(new InventoryId(command.InventoryId), cancellationToken).ConfigureAwait(false);
            if(inventory == null)
            {
                throw new InvalidOperationException("Inventory not found.");
            }

            inventory.RepriceProduct(new ProductId(command.ProductId), command.NewPrice);

            await _inventoryRepository.SaveAsync(inventory, cancellationToken).ConfigureAwait(false);
        }
    }
}