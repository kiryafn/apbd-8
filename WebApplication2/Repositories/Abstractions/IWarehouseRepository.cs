using WebApplication2.Models;

namespace WebApplication2.Repositories.Abstractions;

public interface IWarehouseRepository
{
    public Task<bool> ExistsById(int id, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByOrderId(int orderId, CancellationToken cancellationToken = default);
}