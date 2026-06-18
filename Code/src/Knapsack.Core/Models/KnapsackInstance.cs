namespace Knapsack.Core.Models;

/// <summary>
/// Uma instância do problema da mochila 0/1: capacidade e lista de itens.
/// </summary>
public sealed class KnapsackInstance
{
    public string Name { get; }
    public int Capacity { get; }
    public IReadOnlyList<KnapsackItem> Items { get; }

    public KnapsackInstance(int capacity, IEnumerable<KnapsackItem> items, string? name = null)
    {
        Capacity = capacity;
        Items = items?.ToList() ?? new List<KnapsackItem>();
        Name = string.IsNullOrWhiteSpace(name) ? "instancia-sem-nome" : name;
    }

    public int ItemCount => Items.Count;

    /// <summary>Soma de todos os pesos dos itens (útil para definir capacidades relativas).</summary>
    public int TotalWeight => Items.Sum(i => i.Weight);

    /// <summary>
    /// Valida a instância. Lança <see cref="InvalidOperationException"/> quando inválida.
    /// </summary>
    public void Validate()
    {
        if (Capacity <= 0)
            throw new InvalidOperationException("A capacidade da mochila deve ser maior que zero.");
        if (Items.Count == 0)
            throw new InvalidOperationException("A instância deve conter pelo menos um item.");

        var duplicatedIds = Items.GroupBy(i => i.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicatedIds.Count > 0)
            throw new InvalidOperationException($"Há itens com Id duplicado: {string.Join(", ", duplicatedIds)}.");
    }

    public override string ToString() =>
        $"{Name}: {ItemCount} itens, capacidade={Capacity}, soma dos pesos={TotalWeight}";
}
