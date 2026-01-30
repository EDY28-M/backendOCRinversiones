using AutoMapper;
using backendORCinverisones.Application.DTOs.Products;
using backendORCinverisones.Application.DTOs.Users;
using backendORCinverisones.Application.DTOs.Roles;
using backendORCinverisones.Application.DTOs.Categories;
using backendORCinverisones.Application.DTOs.NombreMarcas;
using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Application.Mappings;

/// <summary>
/// ✅ AutoMapper Profile para mapeo automatizado de entidades a DTOs
/// Reduce código boilerplate y mejora mantenibilidad
/// </summary>
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ============================================
        // Product Mappings
        // ============================================
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.MarcaNombre, opt => opt.MapFrom(src => src.Marca.Nombre));

        CreateMap<CreateProductRequestDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Marca, opt => opt.Ignore());

        CreateMap<UpdateProductRequestDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Marca, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // ============================================
        // User Mappings
        // ============================================
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

        CreateMap<CreateUserRequestDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Role, opt => opt.Ignore());

        CreateMap<UpdateUserRequestDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // ============================================
        // Role Mappings
        // ============================================
        CreateMap<Role, RoleResponseDto>();

        CreateMap<CreateRoleRequestDto, Role>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Users, opt => opt.Ignore());

        CreateMap<UpdateRoleRequestDto, Role>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // ============================================
        // Category Mappings
        // ============================================
        CreateMap<Category, CategoryResponseDto>();

        CreateMap<CreateCategoryRequestDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Products, opt => opt.Ignore());

        CreateMap<UpdateCategoryRequestDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Products, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // ============================================
        // NombreMarca Mappings
        // ============================================
        CreateMap<NombreMarca, NombreMarcaResponseDto>();

        CreateMap<CreateNombreMarcaRequestDto, NombreMarca>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Products, opt => opt.Ignore());

        CreateMap<UpdateNombreMarcaRequestDto, NombreMarca>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Products, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
