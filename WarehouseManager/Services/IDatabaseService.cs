using WarehouseManager.Models;

namespace WarehouseManager.Services
{
    public interface IDatabaseService
    {
        Task<int> AddNewProductAsync(NewProduct newProduct);
    }
}
