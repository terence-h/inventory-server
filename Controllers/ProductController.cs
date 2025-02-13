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
        [FromQuery] int? quantityFrom,
        [FromQuery] int? quantityTo,
        [FromQuery] int? categoryId,
        [FromQuery] DateTime? mfgDateFrom,
        [FromQuery] DateTime? mfgDateTo,
        [FromQuery] DateTime? mfgExpiryDateFrom,
        [FromQuery] DateTime? mfgExpiryDateTo,
        // [FromQuery] DateTime? addedOn,
        [FromQuery] int? page
        )
    {
        var request = new GetProductsRequest
        {
            ProductNo = productNo,
            ProductName = productName,
            Manufacturer = manufacturer,
            BatchNo = batchNo,
            QuantityFrom = quantityFrom,
            QuantityTo = quantityTo,
            CategoryId = categoryId,
            MfgDateFrom = mfgDateFrom,
            MfgDateTo = mfgDateTo,
            MfgExpiryDateFrom = mfgExpiryDateFrom,
            MfgExpiryDateTo = mfgExpiryDateTo,
            // AddedOn = addedOn,
            Page = page
        };
        
        var response = await productRepository.GetProductsAsync(request);

        return Ok(response);
    }
    
    [HttpGet("getProduct/{productId:int}")]
    public async Task<IActionResult> GetProduct([FromRoute] int productId)
    {
        var response = await productRepository.GetProductAsync(productId);

        return Ok(response);
    }

    [HttpPost("addProduct")]
    public async Task<IActionResult> AddProduct([FromBody] AddProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        request.ProductName = request.ProductName.Trim();
        request.ProductNo = request.ProductNo.Trim();
        request.Manufacturer = request.Manufacturer.Trim();
        request.BatchNo = request.BatchNo.Trim();

        var response = await productRepository.AddProductAsync(request);
        
        return response.ProductId > 0 ? Ok(response) : BadRequest(response);
    }

    [HttpPost("editProduct")]
    public async Task<IActionResult> EditProduct([FromBody] EditProductRequest request)
    {
        if (!ModelState.IsValid || request.ProductId < 1)
            return BadRequest(ModelState);
        
        request.ProductName = request.ProductName.Trim();
        request.ProductNo = request.ProductNo.Trim();
        request.Manufacturer = request.Manufacturer.Trim();
        request.BatchNo = request.BatchNo.Trim();

        var response = await productRepository.EditProductAsync(request);
        
        return string.IsNullOrEmpty(response.Message) ? Ok(response) : BadRequest(response);
    }

    [HttpPost("deleteProduct/{productId:int}")]
    public async Task<IActionResult> DeleteProduct([FromRoute] int productId, [FromBody] DeleteProductRequest request)
    {
        if (!ModelState.IsValid || productId < 1)
            return BadRequest(ModelState);
        
        var response = await productRepository.DeleteProductAsync(productId, request);
        
        return response.Success ? Ok(response) : BadRequest(response);
    }
}