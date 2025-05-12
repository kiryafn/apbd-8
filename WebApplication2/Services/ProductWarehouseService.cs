using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication2.DTO;

namespace WebApplication2.Services.Abstractions;

public class ProductWarehouseService : IProductWarehouseService
{
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;
    private readonly IOrderService _orderService;
    private readonly string _connectionString;

    public ProductWarehouseService(
        IProductService productService,
        IWarehouseService warehouseService,
        IOrderService orderService,
        IConfiguration configuration)
    {
        _productService = productService;
        _warehouseService = warehouseService;
        _orderService = orderService;
        _connectionString = configuration.GetConnectionString("Default")!;
    }

    public async Task<int> RegisterProductAsync(ProductWarehouseRequest request)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        // Проверка существования продукта
        if (!await _productService.ExistsAsync(request.IdProduct, conn, tx))
            throw new KeyNotFoundException("Product not found");

        // Проверка существования склада
        if (!await _warehouseService.ExistsAsync(request.IdWarehouse, conn, tx))
            throw new KeyNotFoundException("Warehouse not found");

        // Поиск подходящего заказа
        int? orderId = await _orderService.FindMatchingOrderAsync(request, conn, tx);
        if (orderId == null)
            throw new KeyNotFoundException("No unfulfilled order found for this product and amount");

        // Обновление заказа
        await _orderService.FulfillOrderAsync(orderId.Value, request.CreatedAt, conn, tx);

        // Получение цены продукта
        decimal price = await _productService.GetPriceAsync(request.IdProduct, conn, tx);

        // Вставка в Product_Warehouse
        int insertedId = await InsertAsync(request, orderId.Value, price, conn, tx);

        tx.Commit();
        return insertedId;
    }

    public async Task<int> InsertAsync(ProductWarehouseRequest request, int orderId, decimal unitPrice, SqlConnection conn, SqlTransaction tx) 
    {
        var cmd = new SqlCommand(@"
            INSERT INTO Product_Warehouse 
            (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
            OUTPUT INSERTED.IdProductWarehouse
            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)", conn, tx);

        cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
        cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        cmd.Parameters.AddWithValue("@IdOrder", orderId);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@Price", unitPrice * request.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

        return (int)(await cmd.ExecuteScalarAsync())!;
    }
    
    public async Task<int> CallStoredProcedureAsync(ProductWarehouseRequest request)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand("AddProductToWarehouse", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

        try
        {
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (SqlException ex) when (ex.Number == 51000)
        {
            throw new KeyNotFoundException(ex.Message);
        }
    }
}