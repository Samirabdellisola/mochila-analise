namespace Knapsack.Core.Models;

/// <summary>
/// Resultado da resolução de uma instância por um algoritmo.
/// Peso e utilidade totais são derivados dos itens selecionados.
/// </summary>
public sealed class KnapsackSolution
{
    public string AlgorithmName { get; }
    public IReadOnlyList<KnapsackItem> SelectedItems { get; }
    public int TotalWeight { get; }
    public int TotalUtility { get; }

    /// <summary>True quando o algoritmo garante a solução ótima (exato ou PD).</summary>
    public bool IsOptimal { get; }
    public TimeSpan ElapsedTime { get; }

    public KnapsackSolution(
        string algorithmName,
        IEnumerable<KnapsackItem> selectedItems,
        bool isOptimal,
        TimeSpan elapsedTime)
    {
        AlgorithmName = algorithmName;
        SelectedItems = selectedItems?.ToList() ?? new List<KnapsackItem>();
        TotalWeight = SelectedItems.Sum(i => i.Weight);
        TotalUtility = SelectedItems.Sum(i => i.Utility);
        IsOptimal = isOptimal;
        ElapsedTime = elapsedTime;
    }

    public double ElapsedMilliseconds => ElapsedTime.TotalMilliseconds;

    public override string ToString()
    {
        var ids = string.Join(", ", SelectedItems.Select(i => i.Id));
        return $"[{AlgorithmName}] utilidade={TotalUtility}, peso={TotalWeight}, " +
               $"tempo={ElapsedMilliseconds:F3} ms, ótimo={(IsOptimal ? "sim" : "não")}, itens=[{ids}]";
    }
}
