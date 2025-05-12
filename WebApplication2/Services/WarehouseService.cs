using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

public class WarehouseService
{
    private readonly string _connectionString;

    public WarehouseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int?> AddProductToWarehouseAsync(int productId, int warehouseId, int amount, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("The amount must be greater than 0.");
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Check if the product exists
                        if (!await CheckIfExistsAsync(connection, transaction, "SELECT 1 FROM Products WHERE IdProduct = @IdProduct", new SqlParameter("@IdProduct", productId), cancellationToken))
                        {
                            throw new Exception("Product does not exist.");
                        }

                        // 2. Check if the warehouse exists
                        if (!await CheckIfExistsAsync(connection, transaction, "SELECT 1 FROM Warehouses WHERE IdWarehouse = @IdWarehouse", new SqlParameter("@IdWarehouse", warehouseId), cancellationToken))
                        {
                            throw new Exception("Warehouse does not exist.");
                        }

                        // 3. Check if there is a purchase order in the Order table that matches our request
                        string orderCheckQuery = @"
                            SELECT TOP 1 IdOrder 
                            FROM [Order]
                            WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";
                        
                        var orderId = await GetIdAsync(connection, transaction, orderCheckQuery, new[]
                        {
                            new SqlParameter("@IdProduct", productId),
                            new SqlParameter("@Amount", amount),
                            new SqlParameter("@CreatedAt", createdAt)
                        }, cancellationToken);

                        if (orderId == null)
                        {
                            throw new Exception("No matching purchase order found in the Order table.");
                        }

                        // 4. Check if the order has already been completed
                        if (await CheckIfExistsAsync(connection, transaction, "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder", new SqlParameter("@IdOrder", orderId), cancellationToken))
                        {
                            throw new Exception("The order has already been completed.");
                        }

                        // 5. Update the FullfilledAt column in the Order table
                        string updateOrderQuery = @"
                            UPDATE [Order]
                            SET FullfilledAt = @FullfilledAt
                            WHERE IdOrder = @IdOrder";

                        await ExecuteNonQueryAsync(connection, transaction, updateOrderQuery, new[]
                        {
                            new SqlParameter("@FullfilledAt", DateTime.UtcNow),
                            new SqlParameter("@IdOrder", orderId)
                        }, cancellationToken);

                        // 6. Insert a new record into the Product_Warehouse table
                        string insertProductWarehouseQuery = @"
                            INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                            OUTPUT INSERTED.IdProductWarehouse
                            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";

                        var productPrice = await GetPriceAsync(connection, transaction, productId, cancellationToken);
                        var totalPrice = productPrice * amount;

                        var insertedId = await ExecuteScalarAsync<int>(connection, transaction, insertProductWarehouseQuery, new[]
                        {
                            new SqlParameter("@IdWarehouse", warehouseId),
                            new SqlParameter("@IdProduct", productId),
                            new SqlParameter("@IdOrder", orderId),
                            new SqlParameter("@Amount", amount),
                            new SqlParameter("@Price", totalPrice),
                            new SqlParameter("@CreatedAt", DateTime.UtcNow)
                        }, cancellationToken);

                        // Commit transaction
                        transaction.Commit();

                        // Return the ID of the inserted row
                        return insertedId;
                    }
                    catch
                    {
                        // Rollback transaction in case of error
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    private async Task<bool> CheckIfExistsAsync(SqlConnection connection, SqlTransaction transaction, string query, SqlParameter parameter, CancellationToken cancellationToken)
    {
        using (var command = new SqlCommand(query, connection, transaction))
        {
            command.Parameters.Add(parameter);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null;
        }
    }

    private async Task<int?> GetIdAsync(SqlConnection connection, SqlTransaction transaction, string query, SqlParameter[] parameters, CancellationToken cancellationToken)
    {
        using (var command = new SqlCommand(query, connection, transaction))
        {
            command.Parameters.AddRange(parameters);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result != DBNull.Value ? (int?)Convert.ToInt32(result) : null;
        }
    }

    private async Task<decimal> GetPriceAsync(SqlConnection connection, SqlTransaction transaction, int productId, CancellationToken cancellationToken)
    {
        string query = "SELECT Price FROM Products WHERE IdProduct = @IdProduct";
        using (var command = new SqlCommand(query, connection, transaction))
        {
            command.Parameters.Add(new SqlParameter("@IdProduct", productId));
            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result == null)
            {
                throw new Exception("Failed to retrieve product price.");
            }
            return Convert.ToDecimal(result);
        }
    }

    private async Task<int> ExecuteNonQueryAsync(SqlConnection connection, SqlTransaction transaction, string query, SqlParameter[] parameters, CancellationToken cancellationToken)
    {
        using (var command = new SqlCommand(query, connection, transaction))
        {
            command.Parameters.AddRange(parameters);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task<T> ExecuteScalarAsync<T>(SqlConnection connection, SqlTransaction transaction, string query, SqlParameter[] parameters, CancellationToken cancellationToken)
    {
        using (var command = new SqlCommand(query, connection, transaction))
        {
            command.Parameters.AddRange(parameters);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result == null)
            {
                throw new Exception("Scalar query returned null.");
            }
            return (T)result;
        }
    }
}