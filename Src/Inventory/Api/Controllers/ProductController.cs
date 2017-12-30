using System;
using System.IO;
using System.Threading.Tasks;
using Domain.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ViewModels;
using ViewModels.Queries;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.QueryStack;

namespace Api.Controllers
{
    [Route("api/products")]
    public class ProductController : Controller
    {
        private readonly ICommandAsyncDispatcher _commandDispatcher;
        private readonly IQueryAsyncDispatcher _queryDispatcher;
        
        public ProductController(ICommandAsyncDispatcher commandDispatcher, IQueryAsyncDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery]bool? includeNotForSale)
        {
            if (!includeNotForSale.HasValue)
            {
                includeNotForSale = false;
            }

            ProductListViewModel result = await _queryDispatcher.DispatchAsync<QueryProductListView, ProductListViewModel>(
                                                    new QueryProductListView(includeNotForSale.Value));
                                                    
            return Ok(result);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(Guid productId)
        {
            ProductViewModel result = await _queryDispatcher.DispatchAsync<QueryProductView, ProductViewModel>(new QueryProductView(productId));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterNewProduct([FromBody]RegisterNewProductCommandDto registerNewProductCommand)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _commandDispatcher.DispatchAsync(registerNewProductCommand.ToDomainCommand());
            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> UnregisterProduct([FromRoute]Guid productId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _commandDispatcher.DispatchAsync(new UnregisterProductCommand(productId));
            return Ok();
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct([FromRoute]Guid productId, [FromHeader]string operation, [FromBody]JObject payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            switch (operation)
            {
                case ProductOperationConstants.MarkProductAsForSale:
                {
                    return await InternalMarkProductAsForSale(productId);
                }

                case ProductOperationConstants.MarkProductAsNotForSale:
                {
                    return await InternalMarkProductAsNotForSale(productId);
                }

                case ProductOperationConstants.RepriceProduct:
                {
                    return await InternalRepriceProduct(productId, payload);
                }

                case ProductOperationConstants.AddProductToCategory:
                {
                    return await InternalAddProductToCategory(productId, payload);
                }

                case ProductOperationConstants.RemoveProductFromCategory:
                {
                    return await InternalRemoveProductFromCategory(productId, payload);
                }

                default:
                    return BadRequest($"{operation} operation is not supported. Check 'operation' HTTP header.");
            }
        }

        [HttpPut("{productId}/stock")]
        public async Task<IActionResult> UpdateProductStock([FromRoute]Guid productId, [FromHeader]string operation, [FromBody]JObject payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            switch (operation)
            {
                case ProductOperationConstants.IncreaseProductStock:
                {
                    return await InternalIncreaseProductStock(productId, payload);
                }

                case ProductOperationConstants.DecreaseProductStock:
                {
                    return await InternalDecreaseProductStock(productId, payload);
                }

                default:
                    return BadRequest($"{operation} operation is not supported. Check 'operation' HTTP header.");
            }
        }

        private async Task<IActionResult> InternalMarkProductAsForSale(Guid productId)
        {
            await _commandDispatcher.DispatchAsync(new MarkProductAsForSaleCommand(productId));
            return Ok();
        }

        private async Task<IActionResult> InternalMarkProductAsNotForSale(Guid productId)
        {
            await _commandDispatcher.DispatchAsync(new MarkProductAsNotForSaleCommand(productId));
            return Ok();
        }

        private async Task<IActionResult> InternalRepriceProduct(Guid productId, JObject jsonPayload)
        {
            const string newPriceKey = nameof(RepriceProductCommand.NewPrice);

            JToken newPriceToken;

            if(!jsonPayload.TryGetValue(newPriceKey, StringComparison.OrdinalIgnoreCase, out newPriceToken))
            {
                return BadRequest($"{newPriceKey} is required.");
            }

            await _commandDispatcher.DispatchAsync(new RepriceProductCommand(productId, newPriceToken.ToObject<decimal>()));
            return Ok();
        }

        private async Task<IActionResult> InternalAddProductToCategory(Guid productId, JObject jsonPayload)
        {
            const string categoryNameKey = nameof(AddProductToCategoryCommand.CategoryName);

            JToken categoryNameToken;

            if(!jsonPayload.TryGetValue(categoryNameKey, StringComparison.OrdinalIgnoreCase, out categoryNameToken))
            {
                return BadRequest($"{categoryNameKey} is required.");
            }
            
            await _commandDispatcher.DispatchAsync(new AddProductToCategoryCommand(productId, categoryNameToken.ToObject<string>()));
            return Ok();
        }

        private async Task<IActionResult> InternalRemoveProductFromCategory(Guid productId, JObject jsonPayload)
        {
            const string categoryNameKey = nameof(RemoveProductFromCategoryCommand.CategoryName);

            JToken categoryNameToken;

            if(!jsonPayload.TryGetValue(categoryNameKey, StringComparison.OrdinalIgnoreCase, out categoryNameToken))
            {
                return BadRequest($"{categoryNameKey} is required.");
            }
            
            await _commandDispatcher.DispatchAsync(new RemoveProductFromCategoryCommand(productId, categoryNameToken.ToObject<string>()));
            return Ok();
        }

        private async Task<IActionResult> InternalIncreaseProductStock(Guid productId, JObject jsonPayload)
        {
            JToken quantityToken;

            if(!jsonPayload.TryGetValue("Quantity", StringComparison.OrdinalIgnoreCase, out quantityToken))
            {
                return BadRequest($"Quantity is required.");
            }

            await _commandDispatcher.DispatchAsync(new IncreaseProductStockCommand(productId, quantityToken.ToObject<int>()));
            return Ok();
        }

        private async Task<IActionResult> InternalDecreaseProductStock(Guid productId, JObject jsonPayload)
        { 
            JToken quantityToken;

            if(!jsonPayload.TryGetValue("Quantity", StringComparison.OrdinalIgnoreCase, out quantityToken))
            {
                return BadRequest($"Quantity is required.");
            }

            await _commandDispatcher.DispatchAsync(new DecreaseProductStockCommand(productId, quantityToken.ToObject<int>()));
            return Ok();
        }
    }

    #region DTOs

    public class RegisterNewProductCommandDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public PriceDto ProductPrice { get; set; }

        public RegisterNewProductCommand ToDomainCommand()
        {
            return new RegisterNewProductCommand(ProductId, ProductName, ProductDescription, ProductPrice.Amount, ProductPrice.Currency);
        }

        public class PriceDto
        {
            public decimal Amount { get; set; }
            public string Currency { get; set; }
        }
    }

    public class UnregisterProductCommandDto
    {
        public Guid ProductId { get; set; }

        public UnregisterProductCommand ToDomainCommand()
        {
            return new UnregisterProductCommand(ProductId);
        }
    }

    public class ProductOperationConstants
    {
        // Product
        public const string MarkProductAsForSale = nameof(MarkProductAsForSale);
        public const string MarkProductAsNotForSale = nameof(MarkProductAsNotForSale);
        public const string RepriceProduct = nameof(RepriceProduct);

        // Category
        public const string AddProductToCategory = nameof(AddProductToCategory);
        public const string RemoveProductFromCategory = nameof(RemoveProductFromCategory);

        // Stock
        public const string IncreaseProductStock = nameof(IncreaseProductStock);
        public const string DecreaseProductStock = nameof(DecreaseProductStock);
    }

    #endregion DTOs
}