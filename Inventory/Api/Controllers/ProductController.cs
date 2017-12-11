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
        private readonly Guid _inventoryIdFromConfiguration;
        
        public ProductController(ICommandAsyncDispatcher commandDispatcher, IQueryAsyncDispatcher queryDispatcher, IConfiguration configuration)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
            _inventoryIdFromConfiguration = configuration.GetSection("Inventory").GetValue<Guid>("InventoryId");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery]bool? includeNotForSale)
        {
            if (!includeNotForSale.HasValue)
            {
                includeNotForSale = false;
            }

            ProductListViewModel result = await _queryDispatcher.DispatchAsync<GetAllProductsQuery, ProductListViewModel>(
                                                    new GetAllProductsQuery(includeNotForSale.Value));
                                                    
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            ProductViewModel result = await _queryDispatcher.DispatchAsync<GetProductByIdQuery, ProductViewModel>(new GetProductByIdQuery(id));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterNewProduct([FromBody]RegisterNewProductCommandDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand(_inventoryIdFromConfiguration));
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> UnregisterProduct([FromBody]UnregisterProductCommandDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand(_inventoryIdFromConfiguration));
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromHeader]string operation, [FromRoute]Guid id, [FromBody]JObject jsonPayload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            switch (operation)
            {
                case ProductOperationConstants.MarkProductAsForSale:
                    {
                        return await InternalMarkProductAsForSale(jsonPayload);
                    }

                case ProductOperationConstants.MarkProductAsNotForSale:
                    {
                        return await InternalMarkProductAsNotForSale(jsonPayload);
                    }

                case ProductOperationConstants.RepriceProduct:
                    {
                        return await InternalRepriceProduct(jsonPayload);
                    }

                case ProductOperationConstants.AddProductToCatalog:
                    {
                        return await InternalAddProductToCatalog(jsonPayload);
                    }

                case ProductOperationConstants.RemoveProductFromCatalog:
                    {
                        return await InternalRemoveProductFromCatalog(jsonPayload);
                    }

                default:
                    return BadRequest($"{operation} operation is not supported.");
            }
        }

        private async Task<IActionResult> InternalMarkProductAsForSale(JObject jsonPayload)
        {
            var dto = JsonConvert.DeserializeObject<MarkProductAsForSaleCommandDto>(jsonPayload.ToString());
            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand());
            return Ok();
        }

        private async Task<IActionResult> InternalMarkProductAsNotForSale(JObject jsonPayload)
        {
            var dto = JsonConvert.DeserializeObject<MarkProductAsNotForSaleCommandDto>(jsonPayload.ToString());
            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand());
            return Ok();
        }

        private async Task<IActionResult> InternalRepriceProduct(JObject jsonPayload)
        {
            var dto = JsonConvert.DeserializeObject<RepriceProductCommandDto>(jsonPayload.ToString());
            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand());
            return Ok();
        }

        private async Task<IActionResult> InternalAddProductToCatalog(JObject jsonPayload)
        {
            var dto = JsonConvert.DeserializeObject<AddProductToCatalogCommandDto>(jsonPayload.ToString());
            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand(_inventoryIdFromConfiguration));
            return Ok();
        }

        private async Task<IActionResult> InternalRemoveProductFromCatalog(JObject jsonPayload)
        {
            var dto = JsonConvert.DeserializeObject<RemoveProductFromCatalogCommandDto>(jsonPayload.ToString());
            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand(_inventoryIdFromConfiguration));
            return Ok();
        }
    }

    #region DTOs

    public class RegisterNewProductCommandDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }

        public RegisterNewProductCommand ToDomainCommand(Guid inventoryId)
        {
            return new RegisterNewProductCommand(inventoryId, ProductId, ProductName, ProductDescription, ProductPrice);
        }
    }

    public class UnregisterProductCommandDto
    {
        public Guid ProductId { get; set; }

        public UnregisterProductCommand ToDomainCommand(Guid inventoryId)
        {
            return new UnregisterProductCommand(inventoryId, ProductId);
        }
    }

    public class ProductOperationConstants
    {
        public const string MarkProductAsForSale = nameof(MarkProductAsForSale);
        public const string MarkProductAsNotForSale = nameof(MarkProductAsNotForSale);
        public const string RepriceProduct = nameof(RepriceProduct);
        public const string AddProductToCatalog = nameof(AddProductToCatalog);
        public const string RemoveProductFromCatalog = nameof(RemoveProductFromCatalog);
    }

    public class MarkProductAsForSaleCommandDto
    {
        public Guid ProductId { get; set; }

        public MarkProductAsForSaleCommand ToDomainCommand()
        {
            return new MarkProductAsForSaleCommand(ProductId);
        }
    }

    public class MarkProductAsNotForSaleCommandDto
    {
        public Guid ProductId { get; set; }

        public MarkProductAsNotForSaleCommand ToDomainCommand()
        {
            return new MarkProductAsNotForSaleCommand(ProductId);
        }
    }

    public class RepriceProductCommandDto
    {
        public Guid ProductId { get; set; }
        public decimal NewPrice { get; set; }

        public RepriceProductCommand ToDomainCommand()
        {
            return new RepriceProductCommand(ProductId, NewPrice);
        }
    }

    public class AddProductToCatalogCommandDto
    {
        public Guid ProductId { get; set; }
        public string CatalogName { get; set; }

        public AddProductToCatalogCommand ToDomainCommand(Guid inventoryId)
        {
            return new AddProductToCatalogCommand(inventoryId, ProductId, CatalogName);
        }
    }

    public class RemoveProductFromCatalogCommandDto
    {
        public Guid ProductId { get; set; }
        public string CatalogName { get; set; }

        public RemoveProductFromCatalogCommand ToDomainCommand(Guid inventoryId)
        {
            return new RemoveProductFromCatalogCommand(inventoryId, ProductId, CatalogName);
        }
    }

    #endregion DTOs
}