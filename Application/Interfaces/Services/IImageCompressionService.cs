namespace backendORCinverisones.Application.Interfaces.Services;

public interface IImageCompressionService
{
    /// <summary>
    /// Comprime una imagen en formato Base64.
    /// Redimensiona si es muy grande y ajusta la calidad.
    /// </summary>
    /// <param name="base64Image">Imagen original en Base64</param>
    /// <returns>Imagen comprimida en Base64</returns>
    Task<string?> CompressBase64Async(string? base64Image);
}
