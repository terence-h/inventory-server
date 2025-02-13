using inventory_server.Entities;
using inventory_server.Models;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;

namespace inventory_server.Repositories.Interfaces;

public interface IProductRepository
{
    Task<PagedResult<GetProductResponse>> GetProductsAsync(GetProductsRequest request);
    Task<GetProductResponse> GetProductAsync(int id);
    Task<AddProductResponse> AddProductAsync(AddProductRequest request);
    Task<EditProductResponse> EditProductAsync(EditProductRequest product);
    Task<DeleteProductResponse> DeleteProductAsync(int productId, DeleteProductRequest request);
}