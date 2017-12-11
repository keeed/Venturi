using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class MarkProductAsForSaleCommand : Command
    {
        public Guid ProductId { get; }

        public MarkProductAsForSaleCommand(Guid productId)
        {
            ProductId = productId;
        }
    }

    public class MarkProductAsForSaleCommandHandler : ICommandAsyncHandler<MarkProductAsForSaleCommand>
    {
        private readonly IRepository<Product, ProductId> _productRepository;

        public MarkProductAsForSaleCommandHandler(IRepository<Product, ProductId> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(MarkProductAsForSaleCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetByIdAsync(new ProductId(command.ProductId), cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            product.MarkForSale();

            await _productRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        }
    }
}