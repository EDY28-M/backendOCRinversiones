using FluentValidation;
using backendORCinverisones.Application.DTOs.Products;

namespace backendORCinverisones.Application.Validators;

/// <summary>
/// ✅ FluentValidation para CreateProductRequestDto
/// Validaciones declarativas y reutilizables
/// </summary>
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequestDto>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(100).WithMessage("El código no puede exceder 100 caracteres")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("El código solo puede contener letras mayúsculas, números y guiones");

        RuleFor(x => x.CodigoComer)
            .NotEmpty().WithMessage("El código comercial es requerido")
            .MaximumLength(100).WithMessage("El código comercial no puede exceder 100 caracteres")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("El código comercial solo puede contener letras mayúsculas, números y guiones");

        RuleFor(x => x.Producto)
            .NotEmpty().WithMessage("El nombre del producto es requerido")
            .MaximumLength(300).WithMessage("El nombre del producto no puede exceder 300 caracteres")
            .MinimumLength(3).WithMessage("El nombre del producto debe tener al menos 3 caracteres");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Debe seleccionar una categoría válida");

        RuleFor(x => x.MarcaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una marca válida");

        RuleFor(x => x.Descripcion)
            .MaximumLength(2000).WithMessage("La descripción no puede exceder 2000 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));

        RuleFor(x => x.FichaTecnica)
            .MaximumLength(5000).WithMessage("La ficha técnica no puede exceder 5000 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.FichaTecnica));

        // Validar que al menos una imagen sea válida
        RuleFor(x => x)
            .Must(HaveAtLeastOneImage)
            .WithMessage("Debe proporcionar al menos una imagen válida")
            .WithName("Imágenes");
    }

    private bool HaveAtLeastOneImage(CreateProductRequestDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.ImagenPrincipal) ||
               !string.IsNullOrWhiteSpace(dto.Imagen2) ||
               !string.IsNullOrWhiteSpace(dto.Imagen3) ||
               !string.IsNullOrWhiteSpace(dto.Imagen4);
    }
}
