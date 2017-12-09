using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class AddProductToCatalogCommand : Command
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }
        public string CatalogName { get; }

        public AddProductToCatalogCommand(Guid inventoryId, Guid productId, string catalogName)
        {
            InventoryId = inventoryId;
            ProductId = productId;
            CatalogName = catalogName;
        }
    }

    public class AddProductToCatalogCommandHandler : ICommandAsyncHandler<AddProductToCatalogCommand>
    {
        private readonly IRepository<Inventory, InventoryId> _inventoryRepository;

        public AddProductToCatalogCommandHandler(IRepository<Inventory, InventoryId> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task HandleAsync(AddProductToCatalogCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Inventory inventory = await _inventoryRepository.GetByIdAsync(new InventoryId(command.InventoryId), cancellationToken).ConfigureAwait(false);
            if(inventory == null)
            {
                throw new InvalidOperationException("Inventory not found.");
            }

            inventory.AddProductToCatalog(new ProductId(command.ProductId), command.CatalogName);

            await _inventoryRepository.SaveAsync(inventory, cancellationToken).ConfigureAwait(false);
        }
    }
}