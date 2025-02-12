using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;

namespace inventory_server.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetCategories();
    Task<AddCategoryResponse> AddCategory(AddCategoryRequest request);
    Task<DeleteCategoryResponse> DeleteCategory(int request);
}