namespace Knapsack.Analysis.Experiments;

/// <summary>
/// Estatísticas agregadas de um algoritmo sobre várias instâncias.
/// </summary>
public sealed class AlgorithmAggregate
{
    public string Algorithm { get; }
    public int ExecutionCount { get; }
    public double AverageElapsedMs { get; }

    /// <summary>Erro relativo médio (apenas execuções em que o ótimo era conhecido).</summary>
    public double? AverageRelativeError { get; }

    public AlgorithmAggregate(
        string algorithm,
        int executionCount,
        double averageElapsedMs,
        double? averageRelativeError)
    {
        Algorithm = algorithm;
        ExecutionCount = executionCount;
        AverageElapsedMs = averageElapsedMs;
        AverageRelativeError = averageRelativeError;
    }
}
