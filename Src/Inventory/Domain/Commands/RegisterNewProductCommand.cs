using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class RegisterNewProductCommand : Command
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public string ProductDescription { get; }
        public decimal ProductPrice { get; }
        public string Currency { get; }

        public RegisterNewProductCommand(Guid productId, string productName, string productDescription, decimal productPrice, string currency)
        {
            ProductId = productId;
            ProductName = productName;
            ProductDescription = productDescription;
            ProductPrice = productPrice;
            Currency = currency;
        }
    }

    public class RegisterNewProductCommandHandler : ICommandAsyncHandler<RegisterNewProductCommand>
    {
        private readonly IProductRepository _productRepository;

        public RegisterNewProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public Task HandleAsync(RegisterNewProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            ProductId productId = new ProductId(command.ProductId);
            return _productRepository.SaveAsync(new Product(productId,
                command.ProductName,
                command.ProductDescription,
                new Price(command.ProductPrice, command.Currency),
                new Stock(productId, 0),
                Enumerable.Empty<ProductCategory>(),
                false));
        }
    }
}