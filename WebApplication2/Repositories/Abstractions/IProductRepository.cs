using WebApplication2.Models;

namespace WebApplication2.Repositories.Abstractions;

public interface IProductRepository
{
    public Task<bool> ExistsById(int id, CancellationToken cancellationToken = default);
    public Task<Product> GetById(int id, CancellationToken cancellationToken = default);
}