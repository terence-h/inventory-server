using inventory_server.Entities;
using inventory_server.Models;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;

namespace inventory_server.Repositories.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetProductsAsync(GetProductsRequest request);
    Task<Product?> GetProductByIdAsync(int id);
    Task<AddProductResponse> AddProductAsync(AddProductRequest request);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
}