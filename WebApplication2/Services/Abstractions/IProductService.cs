using Microsoft.Data.SqlClient;

namespace WebApplication2.Services.Abstractions;

public interface IProductService
{
    Task<bool> ExistsAsync(int productId, SqlConnection conn, SqlTransaction tx);
    Task<decimal> GetPriceAsync(int productId, SqlConnection conn, SqlTransaction tx);
}