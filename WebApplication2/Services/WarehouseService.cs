using Microsoft.Data.SqlClient;
using WebApplication2.Services.Abstractions;


namespace WebApplication2.Services;

public class WarehouseService : IWarehouseService
{
    public async Task<bool> ExistsAsync(int warehouseId, SqlConnection conn, SqlTransaction tx)
    {
        var cmd = new SqlCommand("SELECT COUNT(1) FROM Warehouse WHERE IdWarehouse = @Id", conn, tx);
        cmd.Parameters.AddWithValue("@Id", warehouseId);
        return (int)(await cmd.ExecuteScalarAsync()) == 1;
    }
}