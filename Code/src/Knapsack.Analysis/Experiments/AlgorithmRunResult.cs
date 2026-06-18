using Knapsack.Core.Models;

namespace Knapsack.Analysis.Experiments;

/// <summary>
/// Resultado de um único algoritmo sobre uma instância, mais o erro relativo
/// em relação à solução ótima (quando esta é conhecida).
/// </summary>
public sealed class AlgorithmRunResult
{
    public string InstanceName { get; }
    public int ItemCount { get; }
    public int Capacity { get; }
    public string Algorithm { get; }
    public int TotalUtility { get; }
    public int TotalWeight { get; }
    public double ElapsedMs { get; }
    public bool IsOptimal { get; }

    /// <summary>
    /// Erro relativo da utilidade em relação ao ótimo, em [0,1].
    /// Null quando o ótimo é desconhecido. Para a própria solução ótima é 0.
    /// </summary>
    public double? RelativeError { get; }

    public AlgorithmRunResult(
        KnapsackInstance instance,
        KnapsackSolution solution,
        int? optimalUtility)
    {
        InstanceName = instance.Name;
        ItemCount = instance.ItemCount;
        Capacity = instance.Capacity;
        Algorithm = solution.AlgorithmName;
        TotalUtility = solution.TotalUtility;
        TotalWeight = solution.TotalWeight;
        ElapsedMs = solution.ElapsedMilliseconds;
        IsOptimal = solution.IsOptimal;

        if (optimalUtility is { } optimal && optimal > 0)
            RelativeError = (double)(optimal - solution.TotalUtility) / optimal;
        else if (optimalUtility is 0)
            RelativeError = 0d;
        else
            RelativeError = null;
    }
}
