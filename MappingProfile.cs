using AutoMapper;
using inventory_server.Entities;
using inventory_server.Models.Requests;

namespace inventory_server;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AddCategoryRequest, Category>();
        CreateMap<AddProductRequest, Product>();
        CreateMap<EditProductRequest, Product>();
    }
}