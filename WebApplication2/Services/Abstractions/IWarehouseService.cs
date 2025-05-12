using Microsoft.Data.SqlClient;

namespace WebApplication2.Services.Abstractions;


public interface IWarehouseService
{
    Task<bool> ExistsAsync(int warehouseId, SqlConnection conn, SqlTransaction tx);
}