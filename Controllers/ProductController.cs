using inventory_server.Models.Requests;
using inventory_server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace inventory_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IProductRepository productRepository) : ControllerBase
{
    [HttpGet("getProducts")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? productNo,
        [FromQuery] string? productName,
        [FromQuery] string? manufacturer,
        [FromQuery] string? batchNo,
        [FromQuery] int? quantity,
        [FromQuery] int? categoryId,
        [FromQuery] DateTime? mfgDateFrom,
        [FromQuery] DateTime? mfgDateTo,
        [FromQuery] DateTime? mfgExpiryDateFrom,
        [FromQuery] DateTime? mfgExpiryDateTo,
        [FromQuery] DateTime? addedOn,
        [FromQuery] int? page
        )
    {
        var request = new GetProductsRequest
        {
            ProductNo = productNo,
            ProductName = productName,
            Manufacturer = manufacturer,
            BatchNo = batchNo,
            Quantity = quantity,
            CategoryId = categoryId,
            MfgDateFrom = mfgDateFrom,
            MfgDateTo = mfgDateTo,
            MfgExpiryDateFrom = mfgExpiryDateFrom,
            MfgExpiryDateTo = mfgExpiryDateTo,
            AddedOn = addedOn,
            Page = page
        };
        
        var response = await productRepository.GetProductsAsync(request);

        return Ok(response);
    }

    [HttpPost("addProduct")]
    public async Task<IActionResult> AddProduct([FromBody] AddProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await productRepository.AddProductAsync(request);
        
        return Ok(response);
    }
}