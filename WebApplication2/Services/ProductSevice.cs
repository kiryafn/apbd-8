using WebApplication2.Models;
using WebApplication2.Repositories.Abstractions;
using WebApplication2.Services.Abstractions;

namespace WebApplication2.Services;

public class ProductSevice : IProductService
{
    public readonly IProductRepository _productRepository;
    
    public ProductSevice(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    
    
    public async Task<bool> ExistsById(int id)
    {
        if (id < 0) return false;
        return await _productRepository.ExistsById(id);
    }

    public Task<Product> GetById(int id)
    {
        return _productRepository.GetById(id);
    }
}