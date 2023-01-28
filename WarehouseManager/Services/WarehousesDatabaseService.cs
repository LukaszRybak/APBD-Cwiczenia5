using System.Data.Common;
using System.Data.SqlClient;
using WarehouseManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System;
using System.Data;

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
            using var com = new SqlCommand("AddNewProduct", con);
            com.Parameters.AddWithValue("@IdProduct", newProduct.IdProduct);

            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                int idProduct = new();
                decimal price = new();
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        idProduct = (int)dr["IdProduct"];
                        price = Convert.ToDecimal(dr["Price"]);
                    }
                }

                if (idProduct != newProduct.IdProduct)
                {
                    throw new Exception("Product with given Id was not found");
                }

                com.Parameters.Clear();
                com.CommandText = "SELECT * FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
                com.Parameters.AddWithValue("@IdWarehouse", newProduct.IdWarehouse);


                int idWarehouse = new();
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        idWarehouse = (int)dr["IdWarehouse"];
                    }
                }
                if (idWarehouse != newProduct.IdWarehouse)
                {
                    throw new Exception("Warehouse with given Id was not found");

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
                        throw new Exception("Matching order was not found");
                    }
                }
                DateTime newProductCreatedAt = DateTime.ParseExact(newProduct.CreatedAt, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
                int dateComparison = databaseCreatedAt.CompareTo(newProductCreatedAt);

                if (dateComparison >= 0)
                {
                    throw new Exception("Matching order was not found");
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
                        throw new Exception("Cannot register product for an already fulfilled order");
                    }
                }

                Console.WriteLine("ty");

                com.Parameters.Clear();
                com.CommandText = "UPDATE [dbo].[Order] " +
                                  "SET FulfilledAt = GETDATE() " +
                                  "WHERE IdOrder = @idOrder"; ;
                com.Parameters.AddWithValue("@IdOrder", idOrder);

                await com.ExecuteNonQueryAsync();

                com.Parameters.Clear();
                com.CommandText = "INSERT INTO Product_Warehouse " +
                                  "(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) " +
                                  "VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, GETDATE()); ";

                com.Parameters.AddWithValue("@IdProduct", idProduct);
                com.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
                com.Parameters.AddWithValue("@IdOrder", idOrder);
                com.Parameters.AddWithValue("@Amount", newProduct.Amount);
                com.Parameters.AddWithValue("@Price", newProduct.Amount * price);

                await com.ExecuteNonQueryAsync();

                com.Parameters.Clear();
                com.CommandText = "SELECT IdProductWarehouse " +
                                  "FROM Product_Warehouse " +
                                  "WHERE IdOrder = @IdOrder";

                com.Parameters.AddWithValue("@IdOrder", idOrder);

                int idProductWarehouse = new();
                using (var dr = await com.ExecuteReaderAsync())
                {
                    if (dr.HasRows)
                    {
                        while (await dr.ReadAsync())
                        {
                            idProductWarehouse = (int)dr["IdProductWarehouse"];
                        }
                    }
                }

                await tran.CommitAsync();

                return new DatabaseResponse(200, "New product added succesfully with ID: " + idProductWarehouse);
            }
            catch (SqlException exc)
            {
                await tran.RollbackAsync();
                return new DatabaseResponse(500, exc.Message);
            }
            catch (Exception exc)
            {
                await tran.RollbackAsync();

                if (exc.Message == "Product with given Id was not found")
                {
                    return new DatabaseResponse(404, exc.Message);
                }
                else if (exc.Message == "Warehouse with given Id was not found")
                {
                    return new DatabaseResponse(404, exc.Message);
                }
                else if (exc.Message == "Matching order was not found")
                {
                    return new DatabaseResponse(404, exc.Message);
                }
                else if (exc.Message == "Cannot register product for an already fulfilled order")
                {
                    return new DatabaseResponse(400, exc.Message);
                }
                else
                {
                    return new DatabaseResponse(500, exc.Message);
                }
            }
        }

        public async Task<DatabaseResponse> AddNewProduct2Async(NewProduct newProduct)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var com = new SqlCommand("AddProductToWarehouse", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@IdProduct", newProduct.IdProduct);
            com.Parameters.AddWithValue("@IdWarehouse", newProduct.IdWarehouse);
            com.Parameters.AddWithValue("@Amount", newProduct.Amount);
            com.Parameters.AddWithValue("@CreatedAt", newProduct.CreatedAt);


            try
            {

            await con.OpenAsync();
            decimal idProductWarehouse = new();
            using (var dr = await com.ExecuteReaderAsync())
            {
                if (dr.HasRows)
                {
                    while (await dr.ReadAsync())
                    {
                        idProductWarehouse = (decimal)dr["NewId"];
                    }
                }
            }
            return new DatabaseResponse(200, "New product added succesfully with ID: " + idProductWarehouse);
            }
            catch (SqlException exc)
            {
                return new DatabaseResponse(500, exc.Message);
            }
        }
    }
}
