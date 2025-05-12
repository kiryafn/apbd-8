using WebApplication2.DTO;
using WebApplication2.Models;

namespace WebApplication2.Repositories.Abstractions;

public interface IProductWarehouseRepository
{
    public Task<int> CreateProductWarehouse(WarehouseRequestDTO request, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByOrderId(int orderId, CancellationToken cancellationToken = default);
}