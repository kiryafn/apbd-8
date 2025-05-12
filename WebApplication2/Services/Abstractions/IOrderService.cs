using WebApplication2.DTO;
using WebApplication2.Models;

namespace WebApplication2.Services.Abstractions;

public interface IOrderService
{
    public Task<int?> GetOrderIdByProductAsync(int productId, int amount, DateTime createdAt);
    public Task<Order> GetById(int id);
    public Task<bool> UpdateFullfieldAt(int orderId);
}