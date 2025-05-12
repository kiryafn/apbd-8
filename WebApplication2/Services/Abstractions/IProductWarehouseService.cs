using Microsoft.Data.SqlClient;
using WebApplication2.DTO;

namespace WebApplication2.Services.Abstractions;

public interface IProductWarehouseService
{
    Task<int> RegisterProductAsync(ProductWarehouseRequest request); 
    Task<int> InsertAsync(ProductWarehouseRequest request, int orderId, decimal unitPrice, SqlConnection conn, SqlTransaction tx);
    Task<int> CallStoredProcedureAsync(ProductWarehouseRequest request);
}