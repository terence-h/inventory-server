using inventory_server.Models.Requests;
using inventory_server.Models.Response;

namespace inventory_server.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<AddCategoryResponse> AddCategory(AddCategoryRequest request);
    Task<DeleteCategoryResponse> DeleteCategory(int request);
}