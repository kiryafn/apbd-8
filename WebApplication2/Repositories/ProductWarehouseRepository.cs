using Microsoft.Data.SqlClient;
using WebApplication2.DTO;
using WebApplication2.Repositories.Abstractions;
using WebApplication2.Services.Abstractions;

namespace WebApplication2.Repositories;

public class ProductWarehouseRepository : IProductWarehouseRepository
{
    private readonly string _connectionString;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;

    public ProductWarehouseRepository(IConfiguration cfg, IWarehouseRepository warehouseRepository, IProductRepository productRepository, IOrderRepository orderRepository)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg),
                                "Default connection string is missing in configuration");
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    public async Task<int> CreateProductWarehouse(WarehouseRequestDTO request, CancellationToken token = default)
    {
        if (request.Amount < 0) throw new ArgumentException("Amount must be positive");

        if (await _productRepository.ExistsById(request.IdProduct, token)) throw new Exception("Product does not exist.");
        
        if (await _warehouseRepository.ExistsById(request.IdWarehouse, token)) throw new Exception("Warehouse does not exist.");

        var orderId = await _orderRepository.GetOrderIdByProductAsync(request.IdProduct, request.Amount, request.CreatedAt, token);
        
        if (orderId == null) throw new Exception("No matching order found.");

        if (await ExistsByOrderId(orderId.Value, token)) throw new Exception("Order is already completed.");

        await _orderRepository.UpdateFullfieldAt(orderId.Value, token);
        //
        // string query = @"
        //     INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
        //     OUTPUT INSERTED.IdProductWarehouse
        //     VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";
        //
        // var price = await _productRepository.GetProductPriceAsync(productId, cancellationToken);
        // var totalPrice = price * amount;
        //
        // var insertedId = await _repository.ExecuteScalarAsync<int>(query, new[]
        // {
        //     new SqlParameter("@IdWarehouse", warehouseId),
        //     new SqlParameter("@IdProduct", productId),
        //     new SqlParameter("@IdOrder", orderId),
        //     new SqlParameter("@Amount", amount),
        //     new SqlParameter("@Price", totalPrice),
        //     new SqlParameter("@CreatedAt", DateTime.UtcNow)
        // }, cancellationToken);

        // return insertedId
        return 0;
    }

    public async Task<bool> ExistsByOrderId(int orderId, CancellationToken cancellationToken = default)
    {
        const string query = """
                             SELECT 
                                 IIF(EXISTS (SELECT 1 FROM Product_Warehouse WHERE IdOrder = @orderId), 1, 0);
                             """;
        
        SqlConnection connection = new(_connectionString);
        SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@orderId", orderId);
        connection.Open();
        var result = (int)await command.ExecuteScalarAsync(cancellationToken);
        return result == 1;
    }
}