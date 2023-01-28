using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Models;

namespace WarehouseManager.Services
{
    public interface IDatabaseService
    {
        Task<DatabaseResponse> AddNewProductAsync(NewProduct newProduct);
        Task<DatabaseResponse> AddNewProduct2Async(NewProduct newProduct);
    }
}
