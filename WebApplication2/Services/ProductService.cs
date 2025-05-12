using Microsoft.Data.SqlClient;

namespace WebApplication2.Services.Abstractions;

using System.Data.SqlClient;

public class ProductService : IProductService
{
    public async Task<bool> ExistsAsync(int productId, SqlConnection conn, SqlTransaction tx)
    {
        var cmd = new SqlCommand("SELECT COUNT(1) FROM Product WHERE IdProduct = @Id", conn, tx);
        cmd.Parameters.AddWithValue("@Id", productId);
        return (int)(await cmd.ExecuteScalarAsync()) == 1;
    }

    public async Task<decimal> GetPriceAsync(int productId, SqlConnection conn, SqlTransaction tx)
    {
        var cmd = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @Id", conn, tx);
        cmd.Parameters.AddWithValue("@Id", productId);
        return (decimal)(await cmd.ExecuteScalarAsync())!;
    }
}