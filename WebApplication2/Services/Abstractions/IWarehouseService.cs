namespace WebApplication2.Services.Abstractions;

public interface IWarehouseService
{
    public Task<bool> ExistsById(int id);
    public Task<bool> ExistsByOrderId(int orderId);
}