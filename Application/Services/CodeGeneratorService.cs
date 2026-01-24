using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace backendORCinverisones.Application.Services;

public class CodeGeneratorService : ICodeGeneratorService
{
    private readonly IProductRepository _productRepository;
    
    public CodeGeneratorService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    
    public async Task<string> GenerateNextCodigoAsync()
    {
        try
        {
            // Obtener todos los códigos existentes que siguen el formato PR-XXXXX
            var products = await _productRepository.GetAllAsync();
            var existingCodes = products
                .Select(p => p.Codigo)
                .Where(c => c.StartsWith("PR-"))
                .ToList();
            
            if (!existingCodes.Any())
            {
                return "PR-00001";
            }
            
            // Extraer los números de los códigos
            var usedNumbers = new HashSet<int>();
            var regex = new Regex(@"^PR-(\d{5})$");
            
            foreach (var code in existingCodes)
            {
                var match = regex.Match(code);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
                {
                    usedNumbers.Add(number);
                }
            }
            
            // Encontrar el primer número disponible (incluyendo huecos)
            int nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }
            
            return $"PR-{nextNumber:D5}";
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error al generar código automático", ex);
        }
    }
    
    public async Task<string> GenerateNextCodigoComercialAsync()
    {
        try
        {
            // Obtener todos los códigos comerciales existentes
            var products = await _productRepository.GetAllAsync();
            var existingCodes = products
                .Select(p => p.CodigoComer)
                .ToList();
            
            if (!existingCodes.Any())
            {
                return "AA-0001";
            }
            
            // Formato: AA-0001, AA-0002, ..., AA-9999, AB-0001, etc.
            var usedCodes = new HashSet<string>(existingCodes.Where(c => !string.IsNullOrEmpty(c)));
            var regex = new Regex(@"^([A-Z]{2})-(\d{4})$");
            
            // Extraer todos los códigos válidos y sus componentes
            var validCodes = new List<(string letters, int number)>();
            
            foreach (var code in usedCodes)
            {
                var match = regex.Match(code);
                if (match.Success)
                {
                    string letters = match.Groups[1].Value;
                    if (int.TryParse(match.Groups[2].Value, out int number))
                    {
                        validCodes.Add((letters, number));
                    }
                }
            }
            
            // Si no hay códigos válidos, empezar desde AA-0001
            if (!validCodes.Any())
            {
                return "AA-0001";
            }
            
            // Agrupar por letras y encontrar huecos
            var codesByLetters = validCodes
                .GroupBy(c => c.letters)
                .OrderBy(g => g.Key)
                .ToList();
            
            // Buscar hueco en secuencias existentes
            foreach (var group in codesByLetters)
            {
                var usedNumbers = group.Select(c => c.number).OrderBy(n => n).ToList();
                
                // Buscar hueco entre 1 y el máximo usado
                for (int i = 1; i <= usedNumbers.Max(); i++)
                {
                    if (!usedNumbers.Contains(i))
                    {
                        return $"{group.Key}-{i:D4}";
                    }
                }
                
                // Si llegamos aquí y el máximo es menor a 9999, usar el siguiente
                if (usedNumbers.Max() < 9999)
                {
                    return $"{group.Key}-{(usedNumbers.Max() + 1):D4}";
                }
            }
            
            // Si todas las secuencias están completas, incrementar las letras
            string lastLetters = codesByLetters.Last().Key;
            string nextLetters = IncrementLetters(lastLetters);
            
            return $"{nextLetters}-0001";
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error al generar código comercial automático", ex);
        }
    }
    
    public async Task<bool> IsCodigoAvailableAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return false;
            
        try
        {
            var products = await _productRepository.GetAllAsync();
            var exists = products.Any(p => p.Codigo.ToUpper() == codigo.ToUpper());
                
            return !exists;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error al verificar disponibilidad del código: {codigo}", ex);
        }
    }
    
    public async Task<bool> IsCodigoComercialAvailableAsync(string codigoComer)
    {
        if (string.IsNullOrWhiteSpace(codigoComer))
            return false;
            
        try
        {
            var products = await _productRepository.GetAllAsync();
            var exists = products.Any(p => p.CodigoComer.ToUpper() == codigoComer.ToUpper());
                
            return !exists;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error al verificar disponibilidad del código comercial: {codigoComer}", ex);
        }
    }
    
    private string IncrementLetters(string letters)
    {
        // Convertir letras a números (AA=0, AB=1, ..., ZZ=675)
        char[] chars = letters.ToCharArray();
        
        // Incrementar desde la derecha
        for (int i = chars.Length - 1; i >= 0; i--)
        {
            if (chars[i] == 'Z')
            {
                chars[i] = 'A';
                // Continuar con el siguiente dígito
            }
            else
            {
                chars[i] = (char)(chars[i] + 1);
                break;
            }
        }
        
        return new string(chars);
    }
}
