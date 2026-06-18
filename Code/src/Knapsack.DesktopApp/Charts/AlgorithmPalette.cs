using SkiaSharp;

namespace Knapsack.DesktopApp.Charts;

/// <summary>
/// Nomes curtos e cores estaveis por algoritmo, para os graficos ficarem
/// consistentes entre as abas.
/// </summary>
public static class AlgorithmPalette
{
    public static string ShortName(string algorithmName)
    {
        if (algorithmName.StartsWith("Exato", StringComparison.OrdinalIgnoreCase))
            return "Exato";
        if (algorithmName.StartsWith("Programacao", StringComparison.OrdinalIgnoreCase))
            return "PD";
        if (algorithmName.StartsWith("Guloso", StringComparison.OrdinalIgnoreCase))
            return "Guloso";
        return algorithmName;
    }

    public static SKColor ColorFor(string algorithmName) => ShortName(algorithmName) switch
    {
        "Exato" => new SKColor(0x42, 0x85, 0xF4),  // azul
        "PD" => new SKColor(0x34, 0xA8, 0x53),     // verde
        "Guloso" => new SKColor(0xFB, 0xBC, 0x05), // amarelo
        _ => new SKColor(0x9E, 0x9E, 0x9E)
    };
}
