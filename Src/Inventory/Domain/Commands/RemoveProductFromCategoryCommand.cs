using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class RemoveProductFromCategoryCommand : Command
    {
        public Guid ProductId { get; }
        public string CategoryName { get; }

        public RemoveProductFromCategoryCommand(Guid productId, string categoryname)
        {
            ProductId = productId;
            CategoryName = categoryname;
        }
    }

    public class RemoveProductFromCatalogCommandHandler : ICommandAsyncHandler<RemoveProductFromCategoryCommand>
    {
        private readonly IProductRepository _productRepository;

        public RemoveProductFromCatalogCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(RemoveProductFromCategoryCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetProductByIdAsync(new ProductId(command.ProductId), cancellationToken).ConfigureAwait(false);
            if(product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            product.RemoveFromCategory(new ProductCategory(command.CategoryName));

            await _productRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        }
    }
}