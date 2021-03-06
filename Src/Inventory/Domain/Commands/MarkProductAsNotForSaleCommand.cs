using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class MarkProductAsNotForSaleCommand : Command
    {
        public Guid ProductId { get; }

        public MarkProductAsNotForSaleCommand(Guid productId)
        {
            ProductId = productId;
        }
    }

    public class MarkProductAsNotForSaleCommandHandler : ICommandAsyncHandler<MarkProductAsNotForSaleCommand>
    {
        private readonly IProductRepository _productRepository;

        public MarkProductAsNotForSaleCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(MarkProductAsNotForSaleCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetProductByIdAsync(new ProductId(command.ProductId), cancellationToken).ConfigureAwait(false);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            product.MarkNotForSale();

            await _productRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        }
    }
}