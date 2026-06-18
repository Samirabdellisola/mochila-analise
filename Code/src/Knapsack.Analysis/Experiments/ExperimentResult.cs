using Knapsack.Core.Models;

namespace Knapsack.Analysis.Experiments;

/// <summary>
/// Agrega os resultados de todos os algoritmos sobre uma mesma instância.
/// </summary>
public sealed class ExperimentResult
{
    public string InstanceName { get; }
    public int ItemCount { get; }
    public int Capacity { get; }

    public IReadOnlyList<AlgorithmRunResult> Runs { get; }

    /// <summary>Maior utilidade conhecida (de algum algoritmo exato/PD), se houver.</summary>
    public int? OptimalUtility { get; }

    public ExperimentResult(KnapsackInstance instance, IEnumerable<KnapsackSolution> solutions)
    {
        InstanceName = instance.Name;
        ItemCount = instance.ItemCount;
        Capacity = instance.Capacity;

        var solutionList = solutions.ToList();

        // O ótimo vem de qualquer solução marcada como ótima (exato ou PD).
        var optimalSolution = solutionList.FirstOrDefault(s => s.IsOptimal);
        OptimalUtility = optimalSolution?.TotalUtility;

        Runs = solutionList
            .Select(s => new AlgorithmRunResult(instance, s, OptimalUtility))
            .ToList();
    }
}
