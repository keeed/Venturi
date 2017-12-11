using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.Repositories;

namespace Infrastructure.Repositories
{
    public class ProductInMemoryRepository : IProductRepository
    {
        private readonly List<Product> _products = new List<Product>();

        public Task<Product> GetByIdAsync(ProductId productId, CancellationToken ct = default(CancellationToken))
        {
            if (productId == null)
            {
                throw new System.ArgumentNullException(nameof(productId));
            }

            var storedProduct = _products.FirstOrDefault(p => p.Id == productId);
            var state = storedProduct.GetCurrentState();
             // Make a copy to return as result.
            return Task.FromResult(new Product(state.ProductId,
                                               state.InventoryId,
                                               state.ProductName,
                                               state.ProductDescription,
                                               state.Price,
                                               state.Stock,
                                               state.Catalogs,
                                               state.IsForSale,
                                               state.IsUnregistered));
        }

        public Task SaveAsync(Product product, CancellationToken ct = default(CancellationToken))
        {
            if (product == null)
            {
                throw new System.ArgumentNullException(nameof(product));
            }

            _products.RemoveAll(p => p.Id == product.Id);

            _products.Add(product);

            return Task.CompletedTask;
        }
    }
}