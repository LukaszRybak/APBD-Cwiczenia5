using System.Data.Common;
using System.Data.SqlClient;
using WarehouseManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WarehouseManager.Services
{
    public class WarehousesDatabaseService : IDatabaseService
    {
        private IConfiguration _configuration;

        public WarehousesDatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DatabaseResponse> AddNewProductAsync(NewProduct newProduct)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var com = new SqlCommand("SELECT * FROM product WHERE IdProduct = @IdProduct", con);
            com.Parameters.AddWithValue("@IdProduct", newProduct.IdProduct);

            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                int idProductCheck = -1;
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        idProductCheck = (int)dr["IdProduct"];
                    }
                }

                if (idProductCheck != newProduct.IdProduct)
                {
                    return new DatabaseResponse(404, "Product with given Id was not found", -1);
                }

                

                com.Parameters.Clear();
                com.CommandText = "SELECT * FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
                com.Parameters.AddWithValue("@IdWarehouse", newProduct.IdWarehouse);


                int idWarehouseCheck = -1;
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        idWarehouseCheck = (int)dr["IdWarehouse"];
                    }
                }
                if (idWarehouseCheck != newProduct.IdWarehouse)
                {
                    return new DatabaseResponse(404, "Warehouse with given Id was not found", 0);
                }

                com.Parameters.Clear();
                com.CommandText = "SELECT * FROM [dbo].[Order] WHERE IdProduct = @IdProduct";
                com.Parameters.AddWithValue("@IdProduct", newProduct.IdProduct);

                int productOrderedCheck = -1;
                int amountCheck = -1;
                string createdAtCheck = "";
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        productOrderedCheck = (int)dr["IdProduct"];
                        amountCheck = (int)dr["Amount"];
                        createdAtCheck = (string)dr["CreatedAt"];
                    }
                }

                DateTime d1 = DateTime.Parse(createdAtCheck);
                DateTime d2 = DateTime.Parse(newProduct.CreatedAt);
                int dateComparison = d1.CompareTo(d2);

                Console.WriteLine(productOrderedCheck);
                Console.WriteLine(amountCheck);
                Console.WriteLine(dateComparison);


                if (productOrderedCheck != newProduct.IdProduct ||
                    amountCheck != newProduct.Amount ||
                    dateComparison < 0
                    )
                {
                    return new DatabaseResponse(404, "Order for new product was not found", -1);
                }

                await tran.CommitAsync();
            }
            catch (SqlException exc)
            {
                //...
                await tran.RollbackAsync();
            }
            catch (Exception exc)
            {
                //...
                await tran.RollbackAsync();
            }

            return new DatabaseResponse(200, "New product added succesfully with Id: ", 12345);
        }
    }
}
