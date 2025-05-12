using Microsoft.Data.SqlClient;
using WebApplication2.DTO;
using WebApplication2.Services.Abstractions;

namespace WebApplication2.Services;

public class OrderService : IOrderService
{
    public async Task<int?> FindMatchingOrderAsync(ProductWarehouseRequest request, SqlConnection conn, SqlTransaction tx)
    {
        var cmd = new SqlCommand(@"
            SELECT TOP 1 IdOrder FROM [Order] 
            WHERE IdProduct = @IdProduct AND Amount = @Amount AND FulfilledAt IS NULL 
            ORDER BY CreatedAt DESC", conn, tx);
        cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        var result = await cmd.ExecuteScalarAsync();
        return result != null ? (int?)result : null;
    }

    public async Task FulfillOrderAsync(int orderId, DateTime fulfilledAt, SqlConnection conn, SqlTransaction tx)
    {
        var cmd = new SqlCommand("UPDATE [Order] SET FulfilledAt = @time WHERE IdOrder = @id", conn, tx);
        cmd.Parameters.AddWithValue("@time", fulfilledAt);
        cmd.Parameters.AddWithValue("@id", orderId);
        await cmd.ExecuteNonQueryAsync();
    }
}