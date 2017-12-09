using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class CreateCatalogCommand : Command
    {
        public Guid InventoryId { get; }
        public string CatalogName { get; }

        public CreateCatalogCommand(Guid inventoryId, string catalogName)
        {
            InventoryId = inventoryId;
            CatalogName = catalogName;
        }
    }

    public class CreateCatalogCommandHandler : ICommandAsyncHandler<CreateCatalogCommand>
    {
        private readonly IRepository<Inventory, InventoryId> _inventoryRepository;

        public CreateCatalogCommandHandler(IRepository<Inventory, InventoryId> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }
        public async Task HandleAsync(CreateCatalogCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Inventory inventory = await _inventoryRepository.GetByIdAsync(new InventoryId(command.InventoryId), cancellationToken).ConfigureAwait(false);
            if(inventory == null)
            {
                throw new InvalidOperationException("Inventory not found.");
            }

            inventory.CreateNewCatalog(command.CatalogName);

            await _inventoryRepository.SaveAsync(inventory, cancellationToken).ConfigureAwait(false);
        }
    }
}