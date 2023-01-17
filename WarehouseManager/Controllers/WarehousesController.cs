using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Models;
using WarehouseManager.Services;

namespace WarehouseManager.Controllers
{
    [Route("api/warehouses")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {

        private IDatabaseService _databaseService;

        public WarehousesController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost]
        public async Task<IActionResult> AddNewProductAsync([FromBody] NewProduct newProduct)
        {
            var resultId = await _databaseService.AddNewProductAsync(newProduct);
            return Ok("New product added successfully | ID: " + resultId.ToString() );
        }

    }
}
