using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.Repositories;

namespace Domain.Repositories
{
    public class ProductInMemoryRepository : IProductRepository
    {
        private readonly List<Product> _products = new List<Product>();

        public Task<Product> GetProductByIdAsync(ProductId productId, CancellationToken ct = default(CancellationToken))
        {
            if (productId == null)
            {
                throw new System.ArgumentNullException(nameof(productId));
            }

            var storedProduct = _products.FirstOrDefault(p => p.Id == productId);
            if (storedProduct == null)
            {
                return Task.FromResult<Product>(null);
            }

            var state = storedProduct.GetCurrentState();

             // Make a copy to return as result.
            return Task.FromResult(new Product(state));
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