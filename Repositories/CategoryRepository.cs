using AutoMapper;
using inventory_server.Database;
using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;
using inventory_server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Repositories;

public class CategoryRepository(CategoryDbContext dbContext, IMapper mapper) : ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetCategories()
    {
        return await dbContext.Categories.ToListAsync();
    }
    
    public async Task<AddCategoryResponse> AddCategory(AddCategoryRequest request)
    {
        var category = mapper.Map<Category>(request);
        
        try
        {
            await dbContext.AddAsync(category);
        
            var created = await dbContext.SaveChangesAsync();
        
            var isSuccess = created > 0;

            return new AddCategoryResponse
            {
                Success = isSuccess,
                Message = isSuccess ? null : "Failed to add category.",
            };
        }
        catch (Exception ex)
        {
            return new AddCategoryResponse
            {
                Success = false,
                Message = $"ERROR AddCategory: {ex.Message}",
            };
        }
    }

    public async Task<DeleteCategoryResponse> DeleteCategory(int categoryId)
    {
        var category = await dbContext.Categories.FindAsync(categoryId);

        if (category == null)
        {
            return new DeleteCategoryResponse
            {
                Success = false,
                Message = "Category not found.",
            };
        }

        try
        {
            dbContext.Categories.Remove(category);

            var deleted = await dbContext.SaveChangesAsync();

            if (deleted == 0)
            {
                return new DeleteCategoryResponse
                {
                    Success = false,
                    Message = "Failed to delete category.",
                };
            }

            return new DeleteCategoryResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new DeleteCategoryResponse
            {
                Success = false,
                Message = $"ERROR DeleteCategory: {ex.Message}",
            };
        }
    }
}