using Knapsack.Core.Models;
using Knapsack.Core.Solvers;

namespace Knapsack.Analysis.Experiments;

/// <summary>
/// Executa um conjunto de solvers sobre várias instâncias e agrega os
/// resultados (tempo médio e erro relativo médio por algoritmo).
/// </summary>
public sealed class ExperimentRunner
{
    private readonly IReadOnlyList<IKnapsackSolver> _solvers;

    public ExperimentRunner(IEnumerable<IKnapsackSolver> solvers)
    {
        _solvers = solvers?.ToList() ?? throw new ArgumentNullException(nameof(solvers));
        if (_solvers.Count == 0)
            throw new ArgumentException("Informe ao menos um solver.", nameof(solvers));
    }

    /// <summary>
    /// Roda todos os solvers em cada instância. Solvers que lançam exceção
    /// (ex.: exato em instância grande) são ignorados naquela instância.
    /// </summary>
    public IReadOnlyList<ExperimentResult> Run(IEnumerable<KnapsackInstance> instances)
    {
        var results = new List<ExperimentResult>();

        foreach (var instance in instances)
        {
            var solutions = new List<KnapsackSolution>();
            foreach (var solver in _solvers)
            {
                try
                {
                    solutions.Add(solver.Solve(instance));
                }
                catch (InvalidOperationException)
                {
                    // Solver não aplicável a esta instância (ex.: exato com n grande).
                }
            }

            results.Add(new ExperimentResult(instance, solutions));
        }

        return results;
    }

    /// <summary>
    /// Calcula médias por algoritmo a partir de uma lista de resultados.
    /// </summary>
    public static IReadOnlyList<AlgorithmAggregate> Aggregate(IEnumerable<ExperimentResult> results)
    {
        var allRuns = results.SelectMany(r => r.Runs);

        return allRuns
            .GroupBy(r => r.Algorithm)
            .Select(g =>
            {
                var withError = g.Where(r => r.RelativeError.HasValue).ToList();
                double? avgError = withError.Count > 0
                    ? withError.Average(r => r.RelativeError!.Value)
                    : null;

                return new AlgorithmAggregate(
                    g.Key,
                    g.Count(),
                    g.Average(r => r.ElapsedMs),
                    avgError);
            })
            .ToList();
    }
}
