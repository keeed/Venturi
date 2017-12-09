using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class DeleteCatalogCommand : Command
    {
        public Guid InventoryId { get; }
        public string CatalogName { get; }

        public DeleteCatalogCommand(Guid inventoryId, string catalogName)
        {
            InventoryId = inventoryId;
            CatalogName = catalogName;
        }
    }

    public class DeleteCatalogCommandHandler : ICommandAsyncHandler<DeleteCatalogCommand>
    {
        private readonly IRepository<Inventory, InventoryId> _inventoryRepository;

        public DeleteCatalogCommandHandler(IRepository<Inventory, InventoryId> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task HandleAsync(DeleteCatalogCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Inventory inventory = await _inventoryRepository.GetByIdAsync(new InventoryId(command.InventoryId), cancellationToken).ConfigureAwait(false);
            if(inventory == null)
            {
                throw new InvalidOperationException("Inventory not found.");
            }

            inventory.DeleteCatalog(command.CatalogName);

            await _inventoryRepository.SaveAsync(inventory, cancellationToken).ConfigureAwait(false);
        }
    }
}