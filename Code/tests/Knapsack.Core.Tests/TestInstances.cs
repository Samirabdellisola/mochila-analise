using Knapsack.Core.Models;

namespace Knapsack.Core.Tests;

/// <summary>
/// Instâncias com solução ótima conhecida, usadas em vários testes.
/// </summary>
internal static class TestInstances
{
    /// <summary>
    /// Caso clássico: capacidade 50, itens (peso,util) = (10,60),(20,100),(30,120).
    /// Ótimo conhecido = 220 (itens 2 e 3).
    /// </summary>
    public static KnapsackInstance Classic()
    {
        var items = new[]
        {
            new KnapsackItem(1, 10, 60),
            new KnapsackItem(2, 20, 100),
            new KnapsackItem(3, 30, 120)
        };
        return new KnapsackInstance(50, items, "classic");
    }

    public const int ClassicOptimalUtility = 220;
}
