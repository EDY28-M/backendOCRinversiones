namespace backendORCinverisones.Application.Interfaces.Services;

public interface ICodeGeneratorService
{
    /// <summary>
    /// Genera el siguiente código disponible en formato PR-00001
    /// </summary>
    Task<string> GenerateNextCodigoAsync();
    
    /// <summary>
    /// Genera el siguiente código comercial disponible (combinación de letras y números en mayúsculas)
    /// </summary>
    Task<string> GenerateNextCodigoComercialAsync();
    
    /// <summary>
    /// Verifica si un código está disponible
    /// </summary>
    Task<bool> IsCodigoAvailableAsync(string codigo);
    
    /// <summary>
    /// Verifica si un código comercial está disponible
    /// </summary>
    Task<bool> IsCodigoComercialAvailableAsync(string codigoComer);
}
