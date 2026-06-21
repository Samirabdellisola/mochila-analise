using System.Diagnostics;
using Knapsack.Core.Models;

namespace Knapsack.Core.Solvers;

/// <summary>
/// Algoritmo guloso de aproximação para a mochila 0/1 (fator 1/2).
///
/// O guloso puro por razão utilidade/peso é apenas uma heurística e pode ser
/// arbitrariamente ruim: imagine um item leve de razão altíssima mas valor
/// pequeno competindo com um item pesado que sozinho enche a mochila com valor
/// muito maior; o guloso por razão pegaria o primeiro e perderia quase todo o
/// valor disponível.
///
/// Para se tornar um algoritmo de APROXIMAÇÃO com garantia, comparamos duas
/// soluções candidatas e devolvemos a de maior utilidade:
///   1. a do guloso por razão (inclui itens em ordem decrescente de util/peso);
///   2. o item de maior utilidade que cabe sozinho na mochila.
/// Essa combinação garante pelo menos metade (1/2) da utilidade ótima.
///
/// O critério de ordenação é configurável; a garantia de 1/2 vale para o
/// critério padrão (razão utilidade/peso).
/// </summary>
public sealed class GreedyApproximationSolver : IKnapsackSolver
{
    private readonly Comparison<KnapsackItem> _ordering;
    private readonly string _criterionName;

    /// <summary>Cria a aproximação com o critério padrão (razão utilidade/peso).</summary>
    public GreedyApproximationSolver()
        : this((a, b) => b.Ratio.CompareTo(a.Ratio), "razao util/peso")
    {
    }

    /// <param name="ordering">Comparação que define a prioridade (primeiros são tentados antes).</param>
    /// <param name="criterionName">Nome do critério, exibido no nome do algoritmo.</param>
    public GreedyApproximationSolver(Comparison<KnapsackItem> ordering, string criterionName)
    {
        _ordering = ordering ?? throw new ArgumentNullException(nameof(ordering));
        _criterionName = criterionName;
    }

    public string Name => $"Guloso aproximado ({_criterionName})";

    public KnapsackSolution Solve(KnapsackInstance instance)
    {
        instance.Validate();

        var stopwatch = Stopwatch.StartNew();

        // Candidata 1: guloso pelo critério (razão util/peso por padrão).
        var ordered = instance.Items.ToList();
        ordered.Sort(_ordering);

        var greedySelection = new List<KnapsackItem>();
        int greedyUtility = 0;
        int remaining = instance.Capacity;

        foreach (var item in ordered)
        {
            if (item.Weight <= remaining)
            {
                greedySelection.Add(item);
                greedyUtility += item.Utility;
                remaining -= item.Weight;
            }
        }

        // Candidata 2: o item isolado de maior utilidade que cabe na mochila.
        KnapsackItem? bestSingleItem = instance.Items
            .Where(i => i.Weight <= instance.Capacity)
            .OrderByDescending(i => i.Utility)
            .FirstOrDefault();

        // Devolve a melhor das duas candidatas (garante >= 1/2 do ótimo).
        List<KnapsackItem> selected = bestSingleItem is not null && bestSingleItem.Utility > greedyUtility
            ? new List<KnapsackItem> { bestSingleItem }
            : greedySelection;

        stopwatch.Stop();

        return new KnapsackSolution(Name, selected, isOptimal: false, stopwatch.Elapsed);
    }
}
