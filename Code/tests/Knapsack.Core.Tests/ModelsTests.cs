using Knapsack.Core.Models;
using Xunit;

namespace Knapsack.Core.Tests;

public class ModelsTests
{
    [Fact]
    public void KnapsackItem_RejeitaPesoNaoPositivo()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new KnapsackItem(1, 0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new KnapsackItem(1, -5, 10));
    }

    [Fact]
    public void KnapsackItem_RejeitaUtilidadeNegativa()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new KnapsackItem(1, 5, -1));
    }

    [Fact]
    public void KnapsackItem_CalculaRazaoCorretamente()
    {
        var item = new KnapsackItem(1, 4, 10);
        Assert.Equal(2.5, item.Ratio, precision: 5);
    }

    [Fact]
    public void Instance_Validate_FalhaComCapacidadeInvalida()
    {
        var instance = new KnapsackInstance(0, new[] { new KnapsackItem(1, 2, 3) });
        Assert.Throws<InvalidOperationException>(() => instance.Validate());
    }

    [Fact]
    public void Instance_Validate_FalhaSemItens()
    {
        var instance = new KnapsackInstance(10, Array.Empty<KnapsackItem>());
        Assert.Throws<InvalidOperationException>(() => instance.Validate());
    }

    [Fact]
    public void Instance_Validate_FalhaComIdDuplicado()
    {
        var items = new[] { new KnapsackItem(1, 2, 3), new KnapsackItem(1, 4, 5) };
        var instance = new KnapsackInstance(10, items);
        Assert.Throws<InvalidOperationException>(() => instance.Validate());
    }

    [Fact]
    public void Solution_DerivaPesoEUtilidadeDosItens()
    {
        var items = new[] { new KnapsackItem(1, 2, 3), new KnapsackItem(2, 4, 5) };
        var solution = new KnapsackSolution("teste", items, isOptimal: true, TimeSpan.Zero);

        Assert.Equal(6, solution.TotalWeight);
        Assert.Equal(8, solution.TotalUtility);
    }
}
