namespace Knapsack.Core.Generation;

/// <summary>
/// Controla como a capacidade da instância gerada é definida em relação
/// à soma dos pesos dos itens.
/// </summary>
public enum CapacityMode
{
    /// <summary>Capacidade pequena: ~25% da soma dos pesos.</summary>
    Small,

    /// <summary>Capacidade média: ~50% da soma dos pesos.</summary>
    Medium,

    /// <summary>Capacidade grande: ~75% da soma dos pesos.</summary>
    Large
}
