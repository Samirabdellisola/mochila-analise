namespace Knapsack.Core.Models;

/// <summary>
/// Representa um item candidato da mochila 0/1.
/// É imutável: peso e utilidade são validados na construção.
/// </summary>
public sealed class KnapsackItem
{
    public int Id { get; }
    public int Weight { get; }
    public int Utility { get; }

    public KnapsackItem(int id, int weight, int utility)
    {
        if (weight <= 0)
            throw new ArgumentOutOfRangeException(nameof(weight), "O peso deve ser maior que zero.");
        if (utility < 0)
            throw new ArgumentOutOfRangeException(nameof(utility), "A utilidade não pode ser negativa.");

        Id = id;
        Weight = weight;
        Utility = utility;
    }

    /// <summary>Razão utilidade/peso, usada pelo guloso de aproximação.</summary>
    public double Ratio => (double)Utility / Weight;

    public override string ToString() => $"Item #{Id} (peso={Weight}, util={Utility})";
}
