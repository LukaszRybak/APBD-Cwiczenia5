using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManager.Models
{
    public class DatabaseResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public DatabaseResponse(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
