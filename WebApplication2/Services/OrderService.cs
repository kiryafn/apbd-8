using WebApplication2.DTO;
using WebApplication2.Models;
using WebApplication2.Repositories.Abstractions;
using WebApplication2.Services.Abstractions;

namespace WebApplication2.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    
    public OrderService(IOrderRepository orderRepository) {
        _orderRepository = orderRepository;
    }


    public async Task<int?> GetOrderIdByProductAsync(int productId, int amount, DateTime createdAt)
    {
        return await _orderRepository.GetOrderIdByProductAsync(productId, amount, createdAt);
    }

    public async Task<Order> GetById(int id)
    {
        return await _orderRepository.GetById(id);
    }

    public async Task<bool> UpdateFullfieldAt(int orderId)
    {
        return await _orderRepository.UpdateFullfieldAt(orderId);    
    }
}