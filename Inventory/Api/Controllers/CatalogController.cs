using System;
using System.Threading.Tasks;
using Domain.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ViewModels;
using ViewModels.Queries;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.QueryStack;

namespace Api.Controllers
{
    [Route("api/catalogs")]
    public class CatalogController : Controller
    {
        private readonly ICommandAsyncDispatcher _commandDispatcher;
        private readonly IQueryAsyncDispatcher _queryDispatcher;
        private readonly Guid _inventoryIdFromConfiguration;

        public CatalogController(ICommandAsyncDispatcher commandDispatcher, IQueryAsyncDispatcher queryDispatcher, IConfiguration configuration)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
            _inventoryIdFromConfiguration = configuration.GetSection("Inventory").GetValue<Guid>("InventoryId");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCatalogs()
        {
            ProductCatalogListViewModel result = await _queryDispatcher.DispatchAsync<GetAllCatalogsQuery, ProductCatalogListViewModel>(new GetAllCatalogsQuery());
            return Ok(result);
        }

        [HttpGet("{catalogName}")]
        public async Task<IActionResult> GetCatalog(string catalogName)
        {
            ProductCatalogViewModel result = await _queryDispatcher.DispatchAsync<GetProductsInCatalogQuery, ProductCatalogViewModel>(
                new GetProductsInCatalogQuery(catalogName));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCatalog([FromBody]CreateCatalogCommandDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand(_inventoryIdFromConfiguration));

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCatalog([FromBody]DeleteCatalogCommandDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand(_inventoryIdFromConfiguration));

            return Ok();
        }
    }

    #region DTOs

    public class CreateCatalogCommandDto
    {
        public string CatalogName { get; set; }

        public CreateCatalogCommand ToDomainCommand(Guid inventoryId)
        {
            return new CreateCatalogCommand(inventoryId, CatalogName);
        }
    }

    public class DeleteCatalogCommandDto
    {
        public string CatalogName { get; set; }

        public DeleteCatalogCommand ToDomainCommand(Guid inventoryId)
        {
            return new DeleteCatalogCommand(inventoryId, CatalogName);
        }
    }

    #endregion DTOs
}