using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class RemoveProductFromCatalogCommand : Command
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }
        public string CatalogName { get; }

        public RemoveProductFromCatalogCommand(Guid inventoryId, Guid productId, string catalogName)
        {
            InventoryId = inventoryId;
            ProductId = productId;
            CatalogName = catalogName;
        }
    }

    public class RemoveProductFromCatalogCommandHandler : ICommandAsyncHandler<RemoveProductFromCatalogCommand>
    {
        private readonly IRepository<Inventory, InventoryId> _inventoryRepository;

        public RemoveProductFromCatalogCommandHandler(IRepository<Inventory, InventoryId> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task HandleAsync(RemoveProductFromCatalogCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Inventory inventory = await _inventoryRepository.GetByIdAsync(new InventoryId(command.InventoryId), cancellationToken).ConfigureAwait(false);
            if(inventory == null)
            {
                throw new InvalidOperationException("Inventory not found.");
            }

            inventory.RemoveProductFromCatalog(new ProductId(command.ProductId), command.CatalogName);

            await _inventoryRepository.SaveAsync(inventory, cancellationToken).ConfigureAwait(false);
        }
    }
}