using Knapsack.Core.Models;

namespace Knapsack.DesktopApp.ViewModels;

/// <summary>
/// Linha da tabela comparativa exibida na aba "Comparar".
/// </summary>
public sealed class AlgorithmRowViewModel
{
    public string Algorithm { get; }
    public int Utility { get; }
    public int Weight { get; }
    public double ElapsedMs { get; }
    public bool IsOptimal { get; }

    /// <summary>Diferenca percentual para o otimo (texto pronto para exibicao).</summary>
    public string DiffToOptimal { get; }

    /// <summary>Qualidade relativa ao otimo em porcentagem (texto pronto).</summary>
    public string Quality { get; }

    public AlgorithmRowViewModel(KnapsackSolution solution, int? optimalUtility)
    {
        Algorithm = solution.AlgorithmName;
        Utility = solution.TotalUtility;
        Weight = solution.TotalWeight;
        ElapsedMs = solution.ElapsedMilliseconds;
        IsOptimal = solution.IsOptimal;

        if (optimalUtility is { } opt && opt > 0)
        {
            double diff = (double)(opt - solution.TotalUtility) / opt * 100.0;
            DiffToOptimal = diff.ToString("F2") + "%";
            Quality = ((double)solution.TotalUtility / opt * 100.0).ToString("F2") + "%";
        }
        else
        {
            DiffToOptimal = "-";
            Quality = "-";
        }
    }
}
