using System.Data.Common;
using System.Data.SqlClient;
using WarehouseManager.Models;

namespace WarehouseManager.Services
{
    public class WarehousesDatabaseService : IDatabaseService
    {
        private IConfiguration _configuration;

        public WarehousesDatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> AddNewProductAsync(NewProduct newProduct)
        {
            using var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=pgago;Integrated Security=True");
            using var com = new SqlCommand("select * from animal", con);

            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                var list = new List<Animal>();
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        list.Add(new Animal
                        {
                            Name = dr["Name"].ToString(),
                            Description = dr["Description"].ToString()
                        });
                    }
                }

                com.Parameters.Clear();
                com.CommandText = "UPDATE Animal SET Name=Name+'a' WHERE Name=@Name";
                com.Parameters.AddWithValue("@Name", list[0].Name);
                await com.ExecuteNonQueryAsync();

                throw new Exception("Error");

                com.Parameters.Clear();
                com.Parameters.AddWithValue("@Name", list[1].Name);
                await com.ExecuteNonQueryAsync();

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

            return 1;
        }
    }
}
