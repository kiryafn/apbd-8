using Microsoft.Data.SqlClient;

namespace WebApplication2.Repositories.Abstractions;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly string _connectionString;

    public WarehouseRepository(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg),
                                "Default connection string is missing in configuration");
    }
        
    public async Task<bool> ExistsById(int id, CancellationToken token = default)
    {
        const string query = """
                             SELECT 
                                 IIF(EXISTS (SELECT 1 FROM Product WHERE IdWarehouse = @id), 1, 0);   
                             """;
        await using SqlConnection connection = new(_connectionString);
        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        var result = (int)await command.ExecuteScalarAsync(token);
        return result == 1;
    }

    public async Task<bool> ExistsByOrderId(int orderId, CancellationToken cancellationToken = default)
    {
        const string query = """
                             SELECT 
                                 IIF(EXISTS (SELECT 1 FROM Product WHERE IdOrder = @orderId), 1, 0);   
                             """;
        
        await using SqlConnection connection = new(_connectionString);
        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@orderId", orderId);
        connection.Open();
        var result = (int)await command.ExecuteScalarAsync(cancellationToken);
        return result == 1;
    }
}
