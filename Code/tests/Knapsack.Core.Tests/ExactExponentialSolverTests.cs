using Knapsack.Core.Models;
using Knapsack.Core.Solvers;
using Xunit;

namespace Knapsack.Core.Tests;

public class ExactExponentialSolverTests
{
    private readonly ExactExponentialSolver _solver = new();

    [Fact]
    public void ResolveCasoClassicoComOtimoConhecido()
    {
        var solution = _solver.Solve(TestInstances.Classic());

        Assert.Equal(TestInstances.ClassicOptimalUtility, solution.TotalUtility);
        Assert.True(solution.IsOptimal);
        Assert.True(solution.TotalWeight <= TestInstances.Classic().Capacity);
    }

    [Fact]
    public void NenhumItemCabe_RetornaVazio()
    {
        var items = new[] { new KnapsackItem(1, 100, 50), new KnapsackItem(2, 80, 40) };
        var instance = new KnapsackInstance(10, items, "nada-cabe");

        var solution = _solver.Solve(instance);

        Assert.Empty(solution.SelectedItems);
        Assert.Equal(0, solution.TotalUtility);
    }

    [Fact]
    public void RespeitaLimiteDeItens()
    {
        var smallSolver = new ExactExponentialSolver(maxSupportedItems: 3);
        var items = Enumerable.Range(1, 4).Select(i => new KnapsackItem(i, 2, 2));
        var instance = new KnapsackInstance(10, items, "grande");

        Assert.Throws<InvalidOperationException>(() => smallSolver.Solve(instance));
    }
}
