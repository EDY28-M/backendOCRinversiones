using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using backendORCinverisones.Application.Interfaces.Services;

namespace backendORCinverisones.Application.Services;

public class ImageCompressionService : IImageCompressionService
{
    private const int MAX_WIDTH = 1000;
    private const int MAX_HEIGHT = 1000;
    private const int JPG_QUALITY = 70; // Reducido a 70 para mayor ligereza sin perder mucha calidad

    public async Task<string?> CompressBase64Async(string? base64Image)
    {
        if (string.IsNullOrWhiteSpace(base64Image))
            return base64Image;

        // OPTIMIZACION: Si empieza con http/https, es una URL existente, no comprimir
        if (base64Image.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return base64Image;

        // Limpiar encabezado data:image/...;base64, si existe
        var dataIndex = base64Image.IndexOf("base64,");
        string cleanBase64 = dataIndex >= 0 ? base64Image.Substring(dataIndex + 7) : base64Image;

        try
        {
            byte[] imageBytes = Convert.FromBase64String(cleanBase64);

            using (var image = Image.Load(imageBytes))
            {
                // SIEMPRE redimensionamos y comprimimos para estandarizar el tamaño y peso
                // Redimensionar mantenienda el aspect ratio
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(MAX_WIDTH, MAX_HEIGHT),
                    Sampler = KnownResamplers.Lanczos3 // Mejor calidad en redimensionado
                }));

                // Comprimir a JPEG
                using (var outputStream = new MemoryStream())
                {
                    var encoder = new JpegEncoder
                    {
                        Quality = JPG_QUALITY
                    };

                    await image.SaveAsJpegAsync(outputStream, encoder);
                    
                    var compressedBytes = outputStream.ToArray();
                    var compressedBase64 = Convert.ToBase64String(compressedBytes);

                    // Retornar con el prefijo estandar
                    return $"data:image/jpeg;base64,{compressedBase64}";
                }
            }
        }
        catch (Exception ex)
        {
            // En caso de error, retornamos null o el original para no bloquear el proceso, 
            // pero logueamos en consola
            Console.WriteLine($"Error al comprimir imagen: {ex.Message}");
            return base64Image; 
        }
    }
}
