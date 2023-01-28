using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Models;
using WarehouseManager.Services;

namespace WarehouseManager.Controllers
{
    [Route("api/warehouses2")]
    [ApiController]
    public class Warehouses2Controller : ControllerBase
    {

        private IDatabaseService _databaseService;

        public Warehouses2Controller(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost]
        public async Task<IActionResult> AddNewProductAsync([FromBody] NewProduct newProduct)
        {
            var databaseResponse = await _databaseService.AddNewProduct2Async(newProduct);

            if (databaseResponse.StatusCode == 200)
            {
                return Ok(databaseResponse.Message);
            }
            else if (databaseResponse.StatusCode == 404)
            {
                return NotFound(databaseResponse.Message);
            }
            else if (databaseResponse.StatusCode == 400)
            {
                return BadRequest(databaseResponse.Message);
            }
            else return StatusCode(500, databaseResponse.Message);
        }

    }
}
