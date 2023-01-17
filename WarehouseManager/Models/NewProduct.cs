using System.ComponentModel.DataAnnotations;

namespace WarehouseManager.Models
{
    public class NewProduct
    {
        [Required(ErrorMessage = "IdProduct is required")]
        public int IdProduct { get; set; }

        [Required(ErrorMessage = "IdWarehouse is required")]
        public int IdWarehouse { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public int Amount { get; set; }

        [Required(ErrorMessage = "CreatedAt is required")]
        public string CreatedAt { get; set; }

    }
}
