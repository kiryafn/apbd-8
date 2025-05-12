using WebApplication2.DTO;
using WebApplication2.Models;

namespace WebApplication2.Repositories.Abstractions;

public interface IOrderRepository
{
    public Task<int?> GetOrderIdByProductAsync(int productId, int amount, DateTime createdAt, CancellationToken cancellationToken = default);
    public Task<Order> GetById(int id, CancellationToken cancellationToken = default);
    public Task<bool> UpdateFullfieldAt(int orderId, CancellationToken cancellationToken = default);
}