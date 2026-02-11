using FluentValidation;
using backendORCinverisones.Application.DTOs.Products;

namespace backendORCinverisones.Application.Validators;

/// <summary>
/// ✅ FluentValidation para UpdateProductRequestDto
/// </summary>
public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequestDto>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Codigo)
            .MaximumLength(100).WithMessage("El código no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Codigo));

        RuleFor(x => x.CodigoComer)
            .MaximumLength(100).WithMessage("El código comercial no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoComer));

        RuleFor(x => x.Producto)
            .MaximumLength(300).WithMessage("El nombre del producto no puede exceder 300 caracteres")
            .MinimumLength(3).WithMessage("El nombre del producto debe tener al menos 3 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Producto));

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Debe seleccionar una categoría válida")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.MarcaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una marca válida")
            .When(x => x.MarcaId.HasValue);

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
    }

    private static bool BeValidImageUrlOrEmpty(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        var trimmed = url.Trim();
        if (trimmed.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase)) return true;
        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
               (uri.Scheme == "http" || uri.Scheme == "https");
    }
}
