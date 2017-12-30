using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class AddProductToCategoryCommand : Command
    {
        public Guid ProductId { get; }
        public string CategoryName { get; }

        public AddProductToCategoryCommand(Guid productId, string categoryName)
        {
            ProductId = productId;
            CategoryName = categoryName;
        }
    }

    public class AddProductToCategoryCommandHandler : ICommandAsyncHandler<AddProductToCategoryCommand>
    {
        private readonly IProductRepository _productRepository;

        public AddProductToCategoryCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(AddProductToCategoryCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetProductByIdAsync(new ProductId(command.ProductId), cancellationToken).ConfigureAwait(false);
            if(product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            product.AddProductToCategory(new ProductCategory(command.CategoryName));

            await _productRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        }
    }
}