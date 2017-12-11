using System;
using System.Threading.Tasks;
using Domain.Commands;
using Microsoft.AspNetCore.Mvc;
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

        public CatalogController(ICommandAsyncDispatcher commandDispatcher, IQueryAsyncDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCatalogs()
        {
            ProductCatalogListView result = await _queryDispatcher.DispatchAsync<GetAllCatalogsQuery, ProductCatalogListView>(new GetAllCatalogsQuery());
            return Ok(result);
        }

        [HttpGet("{catalogName}")]
        public async Task<IActionResult> GetCatalog(string catalogName)
        {
            ProductCatalogView result = await _queryDispatcher.DispatchAsync<GetProductsInCatalogQuery, ProductCatalogView>(
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

            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand());

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCatalog([FromBody]DeleteCatalogCommandDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand());

            return Ok();
        }
    }

    #region DTOs

    public class CreateCatalogCommandDto
    {
        public Guid InventoryId { get; set; }
        public string CatalogName { get; set; }

        public CreateCatalogCommand ToDomainCommand()
        {
            return new CreateCatalogCommand(InventoryId, CatalogName);
        }
    }

    public class DeleteCatalogCommandDto
    {
        public Guid InventoryId { get; set; }
        public string CatalogName { get; set; }

        public DeleteCatalogCommand ToDomainCommand()
        {
            return new DeleteCatalogCommand(InventoryId, CatalogName);
        }
    }

    #endregion DTOs
}