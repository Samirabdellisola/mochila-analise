using System.Diagnostics;
using Knapsack.Core.Models;

namespace Knapsack.Core.Solvers;

/// <summary>
/// Programação dinâmica clássica para a mochila 0/1.
///
/// dp[i, w] = melhor utilidade usando os primeiros i itens com capacidade w.
/// Recorrência:
///   dp[i, w] = dp[i-1, w]                              (não pega o item i)
///   dp[i, w] = max(dp[i-1, w], dp[i-1, w-peso] + util)  (se couber)
///
/// Optou-se pela tabela 2D (em vez da versão 1D otimizada) por ser mais
/// didática e por permitir reconstruir os itens escolhidos com backtracking.
/// Custo O(n * W) de tempo e memória.
/// </summary>
public sealed class DynamicProgrammingSolver : IKnapsackSolver
{
    public string Name => "Programacao dinamica";

    public KnapsackSolution Solve(KnapsackInstance instance)
    {
        instance.Validate();

        var items = instance.Items;
        int n = items.Count;
        int capacity = instance.Capacity;

        var stopwatch = Stopwatch.StartNew();

        // dp tem (n+1) linhas e (capacity+1) colunas.
        var dp = new int[n + 1, capacity + 1];

        for (int i = 1; i <= n; i++)
        {
            int weight = items[i - 1].Weight;
            int utility = items[i - 1].Utility;

            for (int w = 0; w <= capacity; w++)
            {
                dp[i, w] = dp[i - 1, w];
                if (weight <= w)
                {
                    int withItem = dp[i - 1, w - weight] + utility;
                    if (withItem > dp[i, w])
                        dp[i, w] = withItem;
                }
            }
        }

        // Reconstrução: se dp[i, w] != dp[i-1, w], o item i foi usado.
        var selected = new List<KnapsackItem>();
        int remaining = capacity;
        for (int i = n; i >= 1; i--)
        {
            if (dp[i, remaining] != dp[i - 1, remaining])
            {
                var item = items[i - 1];
                selected.Add(item);
                remaining -= item.Weight;
            }
        }
        selected.Reverse();

        stopwatch.Stop();

        return new KnapsackSolution(Name, selected, isOptimal: true, stopwatch.Elapsed);
    }
}
