using Microsoft.Data.SqlClient;
using WebApplication2.Models;
using WebApplication2.Repositories.Abstractions;

namespace WebApplication2.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg),
                                "Default connection string is missing in configuration");
    }
        
    public async Task<bool> ExistsById(int id, CancellationToken cancellationToken = default)
    {
        const string query = """
                             SELECT 
                                 IIF(EXISTS (SELECT 1 FROM Product WHERE IdProduct = @id), 1, 0);   
                             """;
        SqlConnection connection = new(_connectionString);
        SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        var result = (int)await command.ExecuteScalarAsync(cancellationToken);
        return result == 1;
    }

    public async Task<Product> GetById(int id, CancellationToken cancellationToken = default)
    {
        const string query = """
                             SELECT IdProduct, Name, Price, Description FROM Product WHERE IdProduct = @id;
                             """;
        
        Product? product = null;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand command = new SqlCommand(query, con);
        command.Parameters.AddWithValue("@id", id);
        await con.OpenAsync(cancellationToken);

        var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!reader.HasRows)
            return product;

        while (await reader.ReadAsync(cancellationToken))
        {
            product = new Product()
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
                Description = reader.GetString(3)
            };
        }

        return product;
    }
}