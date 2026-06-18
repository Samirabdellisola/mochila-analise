using System.Diagnostics;
using Knapsack.Core.Models;

namespace Knapsack.Core.Solvers;

/// <summary>
/// Heurística gulosa aproximada.
///
/// Ordena os itens por um critério (por padrão, razão utilidade/peso
/// decrescente) e os inclui enquanto houver capacidade. É rápida (O(n log n))
/// mas não garante a solução ótima.
///
/// O critério de ordenação é configurável: basta passar outra comparação,
/// permitindo experimentar estratégias diferentes.
/// </summary>
public sealed class GreedyHeuristicSolver : IKnapsackSolver
{
    private readonly Comparison<KnapsackItem> _ordering;
    private readonly string _criterionName;

    /// <summary>Cria a heurística com o critério padrão (razão utilidade/peso).</summary>
    public GreedyHeuristicSolver()
        : this((a, b) => b.Ratio.CompareTo(a.Ratio), "razao util/peso")
    {
    }

    /// <param name="ordering">Comparação que define a prioridade (primeiros são tentados antes).</param>
    /// <param name="criterionName">Nome do critério, exibido no nome do algoritmo.</param>
    public GreedyHeuristicSolver(Comparison<KnapsackItem> ordering, string criterionName)
    {
        _ordering = ordering ?? throw new ArgumentNullException(nameof(ordering));
        _criterionName = criterionName;
    }

    public string Name => $"Guloso ({_criterionName})";

    public KnapsackSolution Solve(KnapsackInstance instance)
    {
        instance.Validate();

        var stopwatch = Stopwatch.StartNew();

        var ordered = instance.Items.ToList();
        ordered.Sort(_ordering);

        var selected = new List<KnapsackItem>();
        int remaining = instance.Capacity;

        foreach (var item in ordered)
        {
            if (item.Weight <= remaining)
            {
                selected.Add(item);
                remaining -= item.Weight;
            }
        }

        stopwatch.Stop();

        return new KnapsackSolution(Name, selected, isOptimal: false, stopwatch.Elapsed);
    }
}
