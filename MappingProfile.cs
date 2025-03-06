using AutoMapper;
using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;

namespace inventory_server;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AddCategoryRequest, Category>();
        CreateMap<AddProductRequest, Product>();
        CreateMap<EditProductRequest, Product>();
        CreateMap<AddAuditLogRequest, AuditLog>();
    }
}