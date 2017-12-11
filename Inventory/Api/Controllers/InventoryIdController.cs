using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [Route("api/inventoryId")]
    public class InventoryIdController : Controller
    {
        private readonly IConfigurationSection _inventoryConfigurationSection;

        public InventoryIdController(IConfiguration configuration)
        {
            _inventoryConfigurationSection = configuration.GetSection("Inventory");   
        }

        [HttpGet]
        public IActionResult GetInventoryId()
        {
            return Ok(_inventoryConfigurationSection["InventoryId"]);
        }
    }
}