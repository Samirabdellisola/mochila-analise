using Knapsack.Core.Models;

namespace Knapsack.Core.Generation;

/// <summary>
/// Gera instâncias aleatórias da mochila 0/1, com controle de faixas de
/// peso/utilidade e do tamanho relativo da capacidade.
/// </summary>
public sealed class KnapsackInstanceGenerator
{
    private readonly Random _random;

    public KnapsackInstanceGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public KnapsackInstance GenerateRandomInstance(
        int itemCount,
        int minWeight,
        int maxWeight,
        int minUtility,
        int maxUtility,
        CapacityMode capacityMode,
        string? name = null)
    {
        if (itemCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(itemCount), "A quantidade de itens deve ser positiva.");
        if (minWeight <= 0 || maxWeight < minWeight)
            throw new ArgumentException("Faixa de peso inválida (exige 0 < minWeight <= maxWeight).");
        if (minUtility < 0 || maxUtility < minUtility)
            throw new ArgumentException("Faixa de utilidade inválida (exige 0 <= minUtility <= maxUtility).");

        var items = new List<KnapsackItem>(itemCount);
        int totalWeight = 0;

        for (int id = 1; id <= itemCount; id++)
        {
            int weight = _random.Next(minWeight, maxWeight + 1);
            int utility = _random.Next(minUtility, maxUtility + 1);
            items.Add(new KnapsackItem(id, weight, utility));
            totalWeight += weight;
        }

        int capacity = ComputeCapacity(totalWeight, capacityMode);
        // Garante capacidade mínima viável (pelo menos o menor peso cabe).
        capacity = Math.Max(capacity, items.Min(i => i.Weight));

        var instanceName = name ?? $"rand-n{itemCount}-{capacityMode}";
        return new KnapsackInstance(capacity, items, instanceName);
    }

    private static int ComputeCapacity(int totalWeight, CapacityMode mode)
    {
        double factor = mode switch
        {
            CapacityMode.Small => 0.25,
            CapacityMode.Medium => 0.50,
            CapacityMode.Large => 0.75,
            _ => 0.50
        };

        return (int)Math.Round(totalWeight * factor);
    }
}
