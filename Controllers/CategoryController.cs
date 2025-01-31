using inventory_server.Models.Requests;
using inventory_server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace inventory_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryRepository categoryRepository) : ControllerBase
{
    [HttpPost("addcategory")]
    public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var response = await categoryRepository.AddCategory(request);
        
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("deletecategory/{categoryId:int}")]
    public async Task<IActionResult> DeleteCategory(int categoryId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var response = await categoryRepository.DeleteCategory(categoryId);
        
        return response.Success ? Ok(response) : BadRequest(response);
    }
}