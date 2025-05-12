using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication2.DTO;
using WebApplication2.Models;
using WebApplication2.Repositories.Abstractions;
using WebApplication2.Services.Abstractions;

namespace WebApplication2.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;
    private readonly string _connectionString;

    public OrderRepository(IConfiguration cfg, IProductService productService, IWarehouseService warehouseService)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg),
                                "Default connection string is missing in configuration");
        _productService = productService;
        _warehouseService = warehouseService;
    }

    public async Task<int?> GetOrderIdByProductAsync(int productId, int amount, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        string query = """
                       SELECT TOP 1 IdOrder
                       FROM [Order]
                       WHERE IdProduct = @IdProduct 
                             AND Amount = @Amount 
                             AND CreatedAt > @CreatedAt 
                             AND FullfilledAt IS NULL;
                       """;

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new(query, connection);

        command.Parameters.AddWithValue("@IdProduct", productId);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        // Открываем соединение
        await connection.OpenAsync(cancellationToken);

        // Выполняем запрос
        var result = await command.ExecuteScalarAsync(cancellationToken);
        
        return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
    }

    public async Task<Order> GetById(int id, CancellationToken cancellationToken = default)
    {
        const string query = """
                             SELECT IdOrder, IdProduct, Amount, CreatedAt FROM Order WHERE IdOrder = @id;
                             """;
        
        Order? order = null;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand command = new SqlCommand(query, con);
        command.Parameters.AddWithValue("@id", id);
        await con.OpenAsync(cancellationToken);

        var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!reader.HasRows)
            return order;

        while (await reader.ReadAsync(cancellationToken))
        {
            order = new Order()
            {
                Id = reader.GetInt32(0),
                Product = await _productService.GetById(reader.GetOrdinal("IdProduct")),
                Amount = reader.GetOrdinal("Amount"),
                CreatedAt = reader.GetDateTime(3),
                FullFieldAt = null
            };
        }

        return order;
    }

    public async Task<bool> UpdateFullfieldAt(int orderId, CancellationToken cancellationToken = default)
    {
        string query = @"
        UPDATE Orders
        SET FullfieldAt = @FullfieldAt
        WHERE OrderId = @OrderId";

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@FullfieldAt", SqlDbType.DateTime).Value = DateTime.UtcNow;
                    command.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;

                    int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

                    return affectedRows > 0;
                }
            }
        }
        catch (Exception ex)
        {
            // Обработка ошибок (например, логирование)
            Console.WriteLine($"Error when updating FullfieldAt: {ex.Message}");
            return false;
        }

    }

}