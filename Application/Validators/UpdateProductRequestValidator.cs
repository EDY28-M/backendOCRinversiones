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
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("El código solo puede contener letras mayúsculas, números y guiones")
            .When(x => !string.IsNullOrWhiteSpace(x.Codigo));

        RuleFor(x => x.CodigoComer)
            .MaximumLength(100).WithMessage("El código comercial no puede exceder 100 caracteres")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("El código comercial solo puede contener letras mayúsculas, números y guiones")
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
    }
}
