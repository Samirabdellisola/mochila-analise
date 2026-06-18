using System.Diagnostics;
using Knapsack.Core.Models;

namespace Knapsack.Core.Solvers;

/// <summary>
/// Algoritmo exato exponencial via enumeração de subconjuntos por bitmask.
///
/// Para n itens existem 2^n subconjuntos. Cada bit do contador representa
/// a inclusão (1) ou não (0) de um item. Avaliamos todos respeitando a
/// capacidade e guardamos a maior utilidade. É garantidamente ótimo, mas
/// só é viável para n pequeno (custo O(2^n * n)).
/// </summary>
public sealed class ExactExponentialSolver : IKnapsackSolver
{
    /// <summary>
    /// Limite de itens aceito. Acima disso a enumeração se torna inviável
    /// (2^n cresce rápido); 30 já gera mais de 1 bilhão de combinações.
    /// </summary>
    public int MaxSupportedItems { get; }

    public ExactExponentialSolver(int maxSupportedItems = 25)
    {
        MaxSupportedItems = maxSupportedItems;
    }

    public string Name => "Exato (forca bruta - bitmask)";

    public KnapsackSolution Solve(KnapsackInstance instance)
    {
        instance.Validate();

        if (instance.ItemCount > MaxSupportedItems)
            throw new InvalidOperationException(
                $"O algoritmo exato suporta no máximo {MaxSupportedItems} itens " +
                $"(instância tem {instance.ItemCount}). Use programação dinâmica.");

        var items = instance.Items;
        int n = items.Count;
        long totalSubsets = 1L << n;

        var stopwatch = Stopwatch.StartNew();

        int bestUtility = -1;
        int bestWeight = 0;
        long bestMask = 0;

        for (long mask = 0; mask < totalSubsets; mask++)
        {
            int weight = 0;
            int utility = 0;

            for (int i = 0; i < n; i++)
            {
                if ((mask & (1L << i)) != 0)
                {
                    weight += items[i].Weight;
                    if (weight > instance.Capacity)
                        break; // poda: subconjunto já estourou a capacidade
                    utility += items[i].Utility;
                }
            }

            if (weight <= instance.Capacity && utility > bestUtility)
            {
                bestUtility = utility;
                bestWeight = weight;
                bestMask = mask;
            }
        }

        var selected = new List<KnapsackItem>();
        for (int i = 0; i < n; i++)
        {
            if ((bestMask & (1L << i)) != 0)
                selected.Add(items[i]);
        }

        stopwatch.Stop();

        return new KnapsackSolution(Name, selected, isOptimal: true, stopwatch.Elapsed);
    }
}
