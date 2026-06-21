using Knapsack.Core.Generation;
using Knapsack.Core.Models;
using Knapsack.Core.Solvers;
using Xunit;

namespace Knapsack.Core.Tests;

public class GreedyApproximationSolverTests
{
    private readonly GreedyApproximationSolver _greedy = new();

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
            // Aproximação nunca pode superar o ótimo.
            Assert.True(greedySolution.TotalUtility <= optimal.TotalUtility);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    [InlineData(2026)]
    public void GarantePeloMenosMetadeDoOtimo(int seed)
    {
        var generator = new KnapsackInstanceGenerator(seed);
        var dp = new DynamicProgrammingSolver();

        for (int n = 1; n <= 25; n++)
        {
            var instance = generator.GenerateRandomInstance(n, 1, 40, 1, 60, CapacityMode.Small);

            var greedySolution = _greedy.Solve(instance);
            var optimal = dp.Solve(instance);

            // Garantia do algoritmo de aproximação (fator 1/2).
            Assert.True(greedySolution.TotalUtility * 2 >= optimal.TotalUtility,
                $"n={n}, seed={seed}: guloso={greedySolution.TotalUtility}, otimo={optimal.TotalUtility}");
        }
    }

    [Fact]
    public void SuperaGulosoPuroQuandoUmItemDominaACapacidade()
    {
        // Item leve com razão alta mas valor baixo vs. item que enche a mochila com valor alto.
        var items = new[]
        {
            new KnapsackItem(1, 1, 2),    // razão 2.0, valor 2
            new KnapsackItem(2, 10, 10)   // razão 1.0, valor 10
        };
        var instance = new KnapsackInstance(10, items, "dominante");

        var solution = _greedy.Solve(instance);

        // O guloso puro por razão pegaria so o item 1 (valor 2);
        // a aproximação escolhe o melhor item isolado (valor 10).
        Assert.Equal(10, solution.TotalUtility);
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
        var byUtility = new GreedyApproximationSolver(
            (a, b) => b.Utility.CompareTo(a.Utility),
            "maior utilidade");

        var solution = byUtility.Solve(TestInstances.Classic());

        Assert.Contains("maior utilidade", solution.AlgorithmName);
        Assert.True(solution.TotalWeight <= TestInstances.Classic().Capacity);
    }
}
