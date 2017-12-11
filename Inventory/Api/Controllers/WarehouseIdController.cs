using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [Route("api/warehouseId")]
    public class WarehouseIdController : Controller
    {
        private readonly IConfigurationSection _inventoryConfigurationSection;

        public WarehouseIdController(IConfiguration configuration)
        {
            _inventoryConfigurationSection = configuration.GetSection("Inventory");   
        }

        [HttpGet]
        public IActionResult GetWarehouseId()
        {
            return Ok(_inventoryConfigurationSection["WarehouseId"]);
        }
    }
}