using System.Data.Common;
using System.Data.SqlClient;
using WarehouseManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Globalization;

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
                int idProductCheck = new();
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        idProductCheck = (int)dr["IdProduct"];
                    }
                }

                if (idProductCheck != newProduct.IdProduct)
                {
                    return new DatabaseResponse(404, "Product with given Id was not found", "");
                }



                com.Parameters.Clear();
                com.CommandText = "SELECT * FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
                com.Parameters.AddWithValue("@IdWarehouse", newProduct.IdWarehouse);


                int idWarehouseCheck = new();
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        idWarehouseCheck = (int)dr["IdWarehouse"];
                    }
                }
                if (idWarehouseCheck != newProduct.IdWarehouse)
                {
                    return new DatabaseResponse(404, "Warehouse with given Id was not found", "");
                }

                com.Parameters.Clear();
                com.CommandText = "SELECT * " +
                                  "FROM [dbo].[Order] " +
                                  "WHERE IdProduct = @IdProduct " +
                                  "AND Amount = @Amount ";
                com.Parameters.AddWithValue("@IdProduct", newProduct.IdProduct);
                com.Parameters.AddWithValue("@Amount", newProduct.Amount);

                DateTime databaseCreatedAt = new();
                int idOrder = new();
                using (var dr = await com.ExecuteReaderAsync())
                {
                    if (dr.HasRows)
                    {
                        while (await dr.ReadAsync())
                        {
                            databaseCreatedAt = dr.GetDateTime(dr.GetOrdinal("CreatedAt"));
                            idOrder = (int)dr["IdOrder"];
                        }
                    }
                    else
                    {
                        return new DatabaseResponse(404, "Matching order was not found", "");
                    }
                }
                DateTime newProductCreatedAt = DateTime.ParseExact(newProduct.CreatedAt, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
                int dateComparison = databaseCreatedAt.CompareTo(newProductCreatedAt);

                if (dateComparison >= 0)
                {
                    return new DatabaseResponse(404, "Matching order was not found", "");
                }

                com.Parameters.Clear();
                com.CommandText = "SELECT [IdProductWarehouse] " +
                                  "FROM [dbo].[Product_Warehouse] " +
                                  "WHERE IdOrder = @IdOrder";
                com.Parameters.AddWithValue("@IdOrder", idOrder);

                using (var dr = await com.ExecuteReaderAsync())
                {
                    if (dr.HasRows)
                    {
                        while (await dr.ReadAsync())
                        {
                            string idProductWarehouse = ((int)dr["IdProductWarehouse"]).ToString();
                            return new DatabaseResponse(200, "Order already fulfilled with ID:", idProductWarehouse);
                        }


                    }
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

            return new DatabaseResponse(200, "New product added succesfully with ID:", "12345");
        }
    }
}
