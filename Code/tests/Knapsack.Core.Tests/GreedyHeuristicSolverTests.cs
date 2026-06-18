using Knapsack.Core.Generation;
using Knapsack.Core.Models;
using Knapsack.Core.Solvers;
using Xunit;

namespace Knapsack.Core.Tests;

public class GreedyHeuristicSolverTests
{
    private readonly GreedyHeuristicSolver _greedy = new();

    [Fact]
    public void NaoEhMarcadoComoOtimo()
    {
        var solution = _greedy.Solve(TestInstances.Classic());
        Assert.False(solution.IsOptimal);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    [InlineData(2026)]
    public void SempreRespeitaCapacidade_ENaoUltrapassaOtimo(int seed)
    {
        var generator = new KnapsackInstanceGenerator(seed);
        var dp = new DynamicProgrammingSolver();

        for (int n = 1; n <= 20; n++)
        {
            var instance = generator.GenerateRandomInstance(n, 1, 20, 1, 30, CapacityMode.Small);

            var greedySolution = _greedy.Solve(instance);
            var optimal = dp.Solve(instance);

            Assert.True(greedySolution.TotalWeight <= instance.Capacity);
            // Heurística nunca pode superar o ótimo.
            Assert.True(greedySolution.TotalUtility <= optimal.TotalUtility);
        }
    }

    [Fact]
    public void NaoLancaParaEntradaValida()
    {
        var generator = new KnapsackInstanceGenerator(7);
        var instance = generator.GenerateRandomInstance(30, 1, 50, 0, 100, CapacityMode.Large);

        var exception = Record.Exception(() => _greedy.Solve(instance));
        Assert.Null(exception);
    }

    [Fact]
    public void CriterioDeOrdenacaoConfiguravel()
    {
        // Critério alternativo: maior utilidade absoluta primeiro.
        var byUtility = new GreedyHeuristicSolver(
            (a, b) => b.Utility.CompareTo(a.Utility),
            "maior utilidade");

        var solution = byUtility.Solve(TestInstances.Classic());

        Assert.Contains("maior utilidade", solution.AlgorithmName);
        Assert.True(solution.TotalWeight <= TestInstances.Classic().Capacity);
    }
}
