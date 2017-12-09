using System;
using System.Threading.Tasks;
using Domain.Commands;
using Microsoft.AspNetCore.Mvc;
using Xer.Cqrs.CommandStack;

namespace Api.Controllers
{
    [Route("api/products")]
    public class ProductController : Controller
    {
        private readonly ICommandAsyncDispatcher _commandDispatcher;

        public ProductController(ICommandAsyncDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterNewProduct([FromBody]RegisterNewProductCommandDto dto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand());

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> UnregisterProduct([FromBody]UnregisterProductCommandDto dto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _commandDispatcher.DispatchAsync(dto.ToDomainCommand());

            return Ok();
        }
    }

    public class RegisterNewProductCommandDto
    {
        public Guid InventoryId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }

        public RegisterNewProductCommand ToDomainCommand()
        {
            return new RegisterNewProductCommand(InventoryId, ProductId, ProductName, ProductDescription, ProductPrice);
        }
    }

    public class UnregisterProductCommandDto
    {
        public Guid InventoryId { get; set; }
        public Guid ProductId { get; set; }

        public UnregisterProductCommand ToDomainCommand()
        {
            return new UnregisterProductCommand(InventoryId, ProductId);
        }
    }
}