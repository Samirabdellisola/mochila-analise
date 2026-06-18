# KnapsackSolution - Mochila 0/1

Solução em C# (.NET 8) para resolver e analisar o problema da **mochila 0/1**
(0/1 knapsack), com separação clara de responsabilidades entre lógica de
domínio, análise de experimentos e uma interface gráfica (WPF) para comparar
os resultados visualmente.

## Estrutura

```
Code/
  KnapsackSolution.sln
  Directory.Build.props        Configurações comuns (net8.0, nullable, implicit usings)
  src/
    Knapsack.Core/             Modelos, algoritmos, geração de instâncias
    Knapsack.Analysis/         Experimentos, estatísticas e exportação CSV
    Knapsack.DesktopApp/       Interface gráfica WPF (botões, tabelas e gráficos)
  tests/
    Knapsack.Core.Tests/       Testes unitários (xUnit)
  data/
    instances/                 Instâncias salvas em JSON
    results/                   CSVs gerados pelos experimentos
```

### Namespaces principais

- `Knapsack.Core.Models` - `KnapsackItem`, `KnapsackInstance`, `KnapsackSolution`
- `Knapsack.Core.Solvers` - `IKnapsackSolver` e implementações
- `Knapsack.Core.Generation` - geração e IO (JSON) de instâncias
- `Knapsack.Analysis.Experiments` - execução de experimentos e CSV
- `Knapsack.DesktopApp.ViewModels` / `Knapsack.DesktopApp.Charts` - UI (MVVM) e gráficos

## Algoritmos implementados

Todos implementam a interface comum `IKnapsackSolver` e medem o tempo com `Stopwatch`.

- **Exato exponencial** (`ExactExponentialSolver`): enumera todos os 2^n
  subconjuntos via bitmask, respeitando a capacidade e guardando a maior
  utilidade. Garante o ótimo, mas só é viável para `n` pequeno (limite
  configurável, padrão 25 itens).
- **Programação dinâmica** (`DynamicProgrammingSolver`): tabela 2D
  `dp[i, w]` com a recorrência clássica da mochila 0/1 e reconstrução dos
  itens por backtracking na tabela. Garante o ótimo em tempo O(n·W).
- **Heurística gulosa** (`GreedyHeuristicSolver`): ordena por razão
  utilidade/peso (critério configurável) e inclui itens enquanto cabe.
  Rápida (O(n log n)), porém sem garantia de otimalidade.

## Como compilar

```bash
cd Code
dotnet build
```

## Como rodar

```bash
cd Code
dotnet run --project src/Knapsack.DesktopApp
```

> A interface é em **WPF**, portanto compila e executa apenas no **Windows**.

A janela é dividida em um painel de parâmetros (à esquerda) e três abas:

- **Instância**: parâmetros para gerar instâncias aleatórias (quantidade de
  itens, faixas de peso/utilidade, modo de capacidade, seed opcional) e botões
  para gerar, carregar e salvar instâncias em JSON. Mostra a tabela de itens.
- **Comparar**: executa os três algoritmos na instância atual e mostra uma
  tabela comparativa (utilidade, peso, tempo, diferença para o ótimo e
  qualidade) mais gráficos de barras de utilidade e de tempo por algoritmo.
- **Experimentos / Escalabilidade**: varia o número de itens (n) e plota o
  tempo médio e o erro relativo médio de cada algoritmo, permitindo exportar
  todos os resultados em CSV para `data/results/`.

A UI reaproveita integralmente os algoritmos e a geração de instâncias de
`Knapsack.Core` e a análise/estatística de `Knapsack.Analysis`. Os gráficos
usam a biblioteca [LiveCharts2](https://livecharts.dev).

## Como rodar os testes

```bash
cd Code
dotnet test
```

Os testes cobrem:

- Casos simples com solução ótima conhecida.
- Igualdade entre o algoritmo exato e a programação dinâmica em instâncias pequenas.
- Garantias da heurística gulosa (respeita capacidade, nunca supera o ótimo, não lança exceções).
- Validações dos modelos de domínio.
