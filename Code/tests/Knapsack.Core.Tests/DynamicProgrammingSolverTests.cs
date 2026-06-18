using Knapsack.Core.Generation;
using Knapsack.Core.Solvers;
using Xunit;

namespace Knapsack.Core.Tests;

public class DynamicProgrammingSolverTests
{
    private readonly DynamicProgrammingSolver _dp = new();

    [Fact]
    public void ResolveCasoClassicoComOtimoConhecido()
    {
        var solution = _dp.Solve(TestInstances.Classic());

        Assert.Equal(TestInstances.ClassicOptimalUtility, solution.TotalUtility);
        Assert.True(solution.IsOptimal);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(42)]
    [InlineData(2026)]
    public void ConcordaComExatoEmInstanciasPequenas(int seed)
    {
        var generator = new KnapsackInstanceGenerator(seed);
        var exact = new ExactExponentialSolver();

        for (int n = 1; n <= 12; n++)
        {
            var instance = generator.GenerateRandomInstance(n, 1, 15, 1, 25, CapacityMode.Medium);

            var dpSolution = _dp.Solve(instance);
            var exactSolution = exact.Solve(instance);

            // A utilidade ótima deve coincidir (os itens em si podem variar em empates).
            Assert.Equal(exactSolution.TotalUtility, dpSolution.TotalUtility);
            Assert.True(dpSolution.TotalWeight <= instance.Capacity);
        }
    }
}
