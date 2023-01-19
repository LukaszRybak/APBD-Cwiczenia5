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
            var databaseResponse = await _databaseService.AddNewProductAsync(newProduct);

            if (databaseResponse.StatusCode == 200)
            {
                return Ok(databaseResponse.Message  + databaseResponse.IdProductWarehouse);
            }
            else if (databaseResponse.StatusCode == 404)
            {
                return NotFound(databaseResponse.Message);
            }
            else return StatusCode(500);
        }

    }
}
