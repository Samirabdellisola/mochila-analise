using Knapsack.Core.Models;

namespace Knapsack.Core.Solvers;

/// <summary>
/// Contrato comum a todos os algoritmos de resolução da mochila 0/1.
/// </summary>
public interface IKnapsackSolver
{
    string Name { get; }
    KnapsackSolution Solve(KnapsackInstance instance);
}
