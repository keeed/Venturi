using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class RegisterNewProductCommand : Command
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }
        public string ProductName { get; }
        public string ProductDescription { get; }
        public decimal ProductPrice { get; }

        public RegisterNewProductCommand(Guid inventoryId, Guid productId, string productName, string productDescription, decimal productPrice)
        {
            InventoryId = inventoryId;
            ProductId = productId;
            ProductName = productName;
            ProductDescription = productDescription;
            ProductPrice = productPrice;
        }
    }

    public class RegisterNewProductCommandHandler : ICommandAsyncHandler<RegisterNewProductCommand>
    {
        private readonly IRepository<Inventory, InventoryId> _inventoryRepository;

        public RegisterNewProductCommandHandler(IRepository<Inventory, InventoryId> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task HandleAsync(RegisterNewProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Inventory inventory = await _inventoryRepository.GetByIdAsync(new InventoryId(command.InventoryId), cancellationToken).ConfigureAwait(false);
            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory not found.");
            }

            inventory.RegisterNewProduct(new ProductId(command.ProductId), command.ProductName, command.ProductDescription, command.ProductPrice);

            await _inventoryRepository.SaveAsync(inventory, cancellationToken).ConfigureAwait(false);
        }
    }
}