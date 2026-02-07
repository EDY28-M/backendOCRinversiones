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

        // Validar formato de URLs de imágenes
        RuleFor(x => x.ImagenPrincipal)
            .Must(BeValidImageUrlOrEmpty).WithMessage("La URL de la imagen principal no es válida (debe ser http/https o data:image)")
            .When(x => !string.IsNullOrWhiteSpace(x.ImagenPrincipal));
        RuleFor(x => x.Imagen2)
            .Must(BeValidImageUrlOrEmpty).WithMessage("La URL de la imagen 2 no es válida")
            .When(x => !string.IsNullOrWhiteSpace(x.Imagen2));
        RuleFor(x => x.Imagen3)
            .Must(BeValidImageUrlOrEmpty).WithMessage("La URL de la imagen 3 no es válida")
            .When(x => !string.IsNullOrWhiteSpace(x.Imagen3));
        RuleFor(x => x.Imagen4)
            .Must(BeValidImageUrlOrEmpty).WithMessage("La URL de la imagen 4 no es válida")
            .When(x => !string.IsNullOrWhiteSpace(x.Imagen4));

        // Validar que al menos una imagen sea válida
        RuleFor(x => x)
            .Must(HaveAtLeastOneImage)
            .WithMessage("Debe proporcionar al menos una imagen válida")
            .WithName("Imágenes");
    }

    private static bool BeValidImageUrlOrEmpty(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        var trimmed = url.Trim();
        if (trimmed.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase)) return true;
        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
               (uri.Scheme == "http" || uri.Scheme == "https");
    }

    private bool HaveAtLeastOneImage(CreateProductRequestDto dto)
    {
        // Al menos una imagen debe ser una URL/data URI válida
        return BeValidImageUrlOrEmpty(dto.ImagenPrincipal) && !string.IsNullOrWhiteSpace(dto.ImagenPrincipal) ||
               BeValidImageUrlOrEmpty(dto.Imagen2) && !string.IsNullOrWhiteSpace(dto.Imagen2) ||
               BeValidImageUrlOrEmpty(dto.Imagen3) && !string.IsNullOrWhiteSpace(dto.Imagen3) ||
               BeValidImageUrlOrEmpty(dto.Imagen4) && !string.IsNullOrWhiteSpace(dto.Imagen4);
    }
}
