using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManager.Models
{
    public class DatabaseResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public int IdProductWarehouse { get; set; } = 0;

        public DatabaseResponse(int statusCode, string message, int idProductWarehouse)
        {
            StatusCode = statusCode;
            Message = message;
            IdProductWarehouse = idProductWarehouse;
        }
    }
}
