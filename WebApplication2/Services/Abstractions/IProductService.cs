using WebApplication2.Models;

namespace WebApplication2.Services.Abstractions;

public interface IProductService
{
    public Task<bool> ExistsById(int id);
    public Task<Product> GetById(int id);
}