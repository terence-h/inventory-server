using System.Globalization;
using AutoMapper;
using inventory_server.Database;
using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;
using inventory_server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Repositories;

public class ProductRepository(ProductDbContext context, IMapper mapper) : IProductRepository
{

    public async Task<PagedResult<GetProductResponse>> GetProductsAsync(GetProductsRequest filters)
    {
        IQueryable<Product> products = context.Products
            .Include(p => p.Category);
        
        if (!string.IsNullOrEmpty(filters.ProductNo))
        {
            products = products.Where(p => p.ProductNo.ToLower().Contains(filters.ProductNo.ToLower()));
        }

        if (!string.IsNullOrEmpty(filters.ProductName))
        {
            products = products.Where(p => p.ProductName.ToLower().Contains(filters.ProductName.ToLower()));
        }

        if (!string.IsNullOrEmpty(filters.Manufacturer))
        {
            products = products.Where(p => p.Manufacturer.ToLower().Contains(filters.Manufacturer.ToLower()));
        }
        
        if (!string.IsNullOrEmpty(filters.BatchNo))
        {
            products = products.Where(p => p.Manufacturer.ToLower().Contains(filters.BatchNo.ToLower()));
        }
        
        if (filters.QuantityFrom.HasValue || filters.QuantityTo.HasValue)
        {
            products = products.Where(p => (p.Quantity >= (filters.QuantityFrom ?? 0)) && p.Quantity <= (filters.QuantityTo ?? int.MaxValue));
        }
        
        if (filters.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == filters.CategoryId);
        }

        if (filters.MfgDateFrom.HasValue || filters.MfgDateTo.HasValue)
        {
            products = products.Where(p => (p.MfgDate != null && p.MfgDate.Value >= (filters.MfgDateFrom ?? DateTime.UnixEpoch)) && (p.MfgDate != null && p.MfgDate.Value <= (filters.MfgDateTo ?? DateTime.MaxValue)));
        }
        
        if (filters.MfgExpiryDateFrom.HasValue || filters.MfgExpiryDateTo.HasValue)
        {
            products = products.Where(p => (p.MfgDate != null && p.MfgDate.Value >= (filters.MfgExpiryDateFrom ?? DateTime.UnixEpoch)) && (p.MfgDate != null && p.MfgDate.Value <= (filters.MfgExpiryDateTo ?? DateTime.MaxValue)));
        }
        
        // if (filters.AddedOn.HasValue)
        // {
        //     products = products.Where(p => p.AddedOn != null ? p.AddedOn.Value >= (filters.MfgExpiryDateFrom != null ? filters.MfgExpiryDateFrom.Value : DateTime.UnixEpoch) : false);
        // }
        
        // Pagination logic
        const int pageSize = 10;
        var page = filters.Page ?? 1;
        var totalCount = await products.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedProducts = await products
            .Select(p => new GetProductResponse()
            {
                ProductId = p.ProductId,
                ProductNo = p.ProductNo,
                ProductName = p.ProductName,
                Manufacturer = p.Manufacturer,
                BatchNo = p.BatchNo,
                Quantity = p.Quantity,
                MfgDate = p.MfgDate != null ? p.MfgDate.Value.ToString("dd/MM/yyyy") : null,
                MfgExpiryDate = p.MfgExpiryDate != null ? p.MfgExpiryDate.Value.ToString("dd/MM/yyyy") : null,
                // AddedOn = p.AddedOn != null ? p.AddedOn.Value.ToString("dd/MM/yyyy") : null,
                CategoryId = p.Category.CategoryId,
                CategoryName = p.Category.CategoryName
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<GetProductResponse>
        {
            CurrentPage = page,
            TotalPages = totalPages,
            Items = pagedProducts
        };
    }

    public async Task<GetProductResponse> GetProductAsync(int id)
    {
        var product = await context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            return new GetProductResponse { ProductId = 0 };
        }

        var productResp = new GetProductResponse()
        {
            ProductId = product.ProductId,
            ProductNo = product.ProductNo,
            ProductName = product.ProductName,
            Manufacturer = product.Manufacturer,
            BatchNo = product.BatchNo,
            Quantity = product.Quantity,
            MfgDate = product.MfgDate?.ToString("yyyy-MM-dd"),
            MfgExpiryDate = product.MfgExpiryDate?.ToString("yyyy-MM-dd"),
            // AddedOn = p.AddedOn != null ? p.AddedOn.Value.ToString("dd/MM/yyyy") : null,
            CategoryId = product.Category.CategoryId,
            CategoryName = product.Category.CategoryName
        };

        return productResp;
    }

    public async Task<AddProductResponse> AddProductAsync(AddProductRequest request)
    {
        var product = mapper.Map<Product>(request);
        
        if (context.Products.Any(p =>
                p.ProductNo == product.ProductNo &&
                p.Manufacturer == product.Manufacturer &&
                p.BatchNo == product.BatchNo))
        {
            return new AddProductResponse
                { ProductId = 0, Message = $"'{product.ProductNo}' of Batch No. '{product.BatchNo}' already exists within '{product.Manufacturer}'!" };
        }

        if (product.MfgDate.HasValue)
        {
            product.MfgDate = DateTime.SpecifyKind(product.MfgDate.Value, DateTimeKind.Utc);
        }

        if (product.MfgExpiryDate.HasValue)
        {
            product.MfgExpiryDate = DateTime.SpecifyKind(product.MfgExpiryDate.Value, DateTimeKind.Utc);
        }
        
        try
        {
            context.Products.Add(product);

            var created = await context.SaveChangesAsync();

            if (created > 0)
            {
                return new AddProductResponse
                    { ProductId = product.ProductId, Message = $"{product.ProductNo} - {product.ProductName} added successfully." };
            }

            return new AddProductResponse
                { ProductId = 0, Message = $"Error adding {product.ProductName}." };
        }
        catch (Exception ex)
        {
            return new AddProductResponse { ProductId = 0, Message = $"ERROR AddProductAsync: {ex.Message}" };
        }
    }

    public async Task<EditProductResponse> EditProductAsync(EditProductRequest request)
    {
        var productToEdit = await context.Products.FindAsync(request.ProductId);

        if (productToEdit == null)
        {
            return new EditProductResponse { ProductId = request.ProductId, Message = "Product does not exist!"};
        }
        
        if (context.Products.Any(p =>
                p.ProductId != request.ProductId &&
                p.ProductNo == request.ProductNo &&
                p.Manufacturer == request.Manufacturer &&
                p.BatchNo == request.BatchNo))
        {
            return new EditProductResponse
                { ProductId = 0, Message = $"'{request.ProductNo} of Batch No. '{request.BatchNo}' already exists within '{request.Manufacturer}'!" };
        }

        if (request.MfgDate.HasValue)
        {
            request.MfgDate = DateTime.SpecifyKind(request.MfgDate.Value, DateTimeKind.Utc);
        }

        if (request.MfgExpiryDate.HasValue)
        {
            request.MfgExpiryDate = DateTime.SpecifyKind(request.MfgExpiryDate.Value, DateTimeKind.Utc);
        }
        
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                productToEdit.ProductNo = request.ProductNo;
                productToEdit.ProductName = request.ProductName;
                productToEdit.Manufacturer = request.Manufacturer;
                productToEdit.BatchNo = request.BatchNo;
                productToEdit.Quantity = request.Quantity;
                productToEdit.CategoryId = request.CategoryId;
                productToEdit.MfgDate = request.MfgDate;
                productToEdit.MfgExpiryDate = request.MfgExpiryDate;
                // context.Products.Update(productToEdit);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new EditProductResponse { ProductId = request.ProductId, Message = $"ERROR EditProductAsync: {ex.Message}" };
            }
        }
        
        return new EditProductResponse
            { ProductId = request.ProductId };
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await context.Products.FindAsync(id);
        
        if (product != null)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }
    }
}