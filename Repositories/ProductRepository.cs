using System.Globalization;
using AutoMapper;
using inventory_server.Database;
using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;
using inventory_server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Repositories;

public class ProductRepository(ProductDbContext dbContext, IMapper mapper, IAuditRepository auditRepository) : IProductRepository
{

    public async Task<PagedResult<GetProductResponse>> GetProductsAsync(GetProductsRequest filters)
    {
        IQueryable<Product> products = dbContext.Products
            .Include(p => p.Category)
            .AsNoTracking();
        
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
            products = products.Where(p => p.BatchNo.ToLower().Contains(filters.BatchNo.ToLower()));
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
            products = products.Where(p => (p.MfgDate != null && p.MfgDate.Value >= (filters.MfgDateFrom != null ? DateTime.SpecifyKind(filters.MfgDateFrom.Value, DateTimeKind.Utc) : DateTime.UnixEpoch)) && (p.MfgDate != null && p.MfgDate.Value <= (filters.MfgDateTo != null ? DateTime.SpecifyKind(filters.MfgDateTo.Value, DateTimeKind.Utc) : DateTime.MaxValue)));
        }
        
        if (filters.MfgExpiryDateFrom.HasValue || filters.MfgExpiryDateTo.HasValue)
        {
            products = products.Where(p => (p.MfgDate != null && p.MfgDate.Value >= (filters.MfgExpiryDateFrom != null ? DateTime.SpecifyKind(filters.MfgExpiryDateFrom.Value, DateTimeKind.Utc) : DateTime.UnixEpoch)) && (p.MfgDate != null && p.MfgDate.Value <= (filters.MfgExpiryDateTo != null ? DateTime.SpecifyKind(filters.MfgExpiryDateTo.Value, DateTimeKind.Utc) : DateTime.MaxValue)));
        }
        
        // Pagination logic
        const int pageSize = 10;
        var totalCount = await products.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        var page = Math.Clamp(filters.Page ?? 1, 1, totalPages);

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
        var product = await dbContext.Products
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
        
        if (dbContext.Products.Any(p =>
                p.ProductNo == product.ProductNo &&
                p.Manufacturer == product.Manufacturer &&
                p.BatchNo == product.BatchNo))
        {
            await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
            {
                AuditTypeId = Globals.AuditType.AddProduct,
                AuditContent = $"Failed to add product: '{product.ProductNo}' of Batch No. '{product.BatchNo}' already exists within '{product.Manufacturer}'",
                ActionBy = request.Username,
                Date = DateTime.Now
            });
            
            return new AddProductResponse
                { ProductId = 0, Message = $"'{product.ProductNo}' of Batch No. '{product.BatchNo}' already exists within '{product.Manufacturer}'" };
        }

        if (product.MfgDate.HasValue)
        {
            product.MfgDate = DateTime.SpecifyKind(product.MfgDate.Value, DateTimeKind.Utc);
        }

        if (product.MfgExpiryDate.HasValue)
        {
            product.MfgExpiryDate = DateTime.SpecifyKind(product.MfgExpiryDate.Value, DateTimeKind.Utc);
        }
        
        await using (var transaction = await dbContext.Database.BeginTransactionAsync())
        {
            dbContext.Products.Add(product);
            var created = await dbContext.SaveChangesAsync();

            if (created == 0)
            {
                var auditLogId = await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
                {
                    AuditTypeId = Globals.AuditType.AddProduct,
                    AuditContent = $"Failed to add product: {product.ProductName}",
                    ActionBy = request.Username,
                    Date = DateTime.Now
                });
                
                await transaction.RollbackAsync();
                return new AddProductResponse
                { 
                    ProductId = 0, 
                    Message = $"Failed to add product: {product.ProductName}. Audit ID: {auditLogId}" 
                };
            }
            
            await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
            {
                AuditTypeId = Globals.AuditType.AddProduct,
                AuditContent = $"[AddProductSuccess]ProductId:{product.ProductId},{request.ToString()}",
                ActionBy = request.Username,
                Date = DateTime.Now
            });
            
            await transaction.CommitAsync();
    
            return new AddProductResponse 
            { 
                ProductId = product.ProductId, 
                Message = "Product added successfully" 
            };
        }
    }

    public async Task<EditProductResponse> EditProductAsync(EditProductRequest request)
    {
        var productToEdit = await dbContext.Products.FindAsync(request.ProductId);

        if (productToEdit == null)
        {
            await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
            {
                AuditTypeId = Globals.AuditType.EditProduct,
                AuditContent = $"Editing product failed - Product does not exist",
                ActionBy = request.Username,
                Date = DateTime.Now
            });
            
            return new EditProductResponse { ProductId = request.ProductId, Message = "Product does not exist"};
        }
        
        if (dbContext.Products.Any(p =>
                p.ProductId != request.ProductId &&
                p.ProductNo == request.ProductNo &&
                p.Manufacturer == request.Manufacturer &&
                p.BatchNo == request.BatchNo))
        {
            await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
            {
                AuditTypeId = Globals.AuditType.EditProduct,
                AuditContent = $"Editing product failed - '{request.ProductName}'/'{request.ProductNo}' of Batch No. '{request.BatchNo}' already exists within '{request.Manufacturer}",
                ActionBy = request.Username,
                Date = DateTime.Now
            });
            
            return new EditProductResponse
                { ProductId = 0, Message = $"'{request.ProductName}'/'{request.ProductNo}' of Batch No. '{request.BatchNo}' already exists within '{request.Manufacturer}'" };
        }

        if (request.MfgDate.HasValue)
        {
            request.MfgDate = DateTime.SpecifyKind(request.MfgDate.Value, DateTimeKind.Utc);
        }

        if (request.MfgExpiryDate.HasValue)
        {
            request.MfgExpiryDate = DateTime.SpecifyKind(request.MfgExpiryDate.Value, DateTimeKind.Utc);
        }
        
        await using (var transaction = await dbContext.Database.BeginTransactionAsync())
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
                // dbContext.Products.Update(productToEdit);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
                {
                    AuditTypeId = Globals.AuditType.EditProduct,
                    AuditContent = $"Editing product failed - {ex.Message}",
                    ActionBy = request.Username,
                    Date = DateTime.Now
                });
                return new EditProductResponse { ProductId = request.ProductId, Message = $"ERROR EditProductAsync: {ex.Message}" };
            }
        }
        
        await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
        {
            AuditTypeId = Globals.AuditType.EditProduct,
            AuditContent = $"[EditProductSuccess]{request.ToString()}",
            ActionBy = request.Username,
            Date = DateTime.Now
        });
        
        return new EditProductResponse
            { ProductId = request.ProductId };
    }

    public async Task<DeleteProductResponse> DeleteProductAsync(int productId, DeleteProductRequest request)
    {
        var product = await dbContext.Products.FindAsync(productId);
        
        if (product == null)
        {
            await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
            {
                AuditTypeId = Globals.AuditType.DeleteProduct,
                AuditContent = $"Failed to delete product {productId} - Product does not exist",
                ActionBy = request.Username,
                Date = DateTime.Now
            });
            
            return new DeleteProductResponse
            {
                Success = false,
                Message = "Product does not exist"
            };
        }
        
        await using (var transaction = await dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                dbContext.Products.Remove(product);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                
                await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
                {
                    AuditTypeId = Globals.AuditType.DeleteProduct,
                    AuditContent = $"Failed to delete product {productId} - {ex.Message}",
                    ActionBy = request.Username,
                    Date = DateTime.Now
                });
                return new DeleteProductResponse { Success = false, Message = $"ERROR DeleteProductAsync: {ex.Message}" };
            }
        }
        
        await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
        {
            AuditTypeId = Globals.AuditType.DeleteProduct,
            AuditContent = $"[DeleteProductSuccess]ProductId:{product.ProductId},ProductNo:{product.ProductNo},ProductName:{product.ProductName},Manufacturer:{product.Manufacturer},BatchNo:{product.BatchNo},Quantity:{product.Quantity},CategoryId:{product.CategoryId},MfgDate:{product.MfgDate},MfgExpiryDate:{product.MfgExpiryDate}",
            ActionBy = request.Username,
            Date = DateTime.Now
        });

        return new DeleteProductResponse
        {
            Success = true,
            Message = $"'{product.ProductName}'/'{product.ProductNo}' of Batch No. '{product.BatchNo}' from '{product.Manufacturer}' has been deleted"
        };
    }
}