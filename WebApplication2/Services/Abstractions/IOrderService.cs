using Microsoft.Data.SqlClient;
using WebApplication2.DTO;

namespace WebApplication2.Services.Abstractions;

public interface IOrderService
{
    Task<int?> FindMatchingOrderAsync(ProductWarehouseRequest request, SqlConnection conn, SqlTransaction tx);
    Task FulfillOrderAsync(int orderId, DateTime fulfilledAt, SqlConnection conn, SqlTransaction tx);
}