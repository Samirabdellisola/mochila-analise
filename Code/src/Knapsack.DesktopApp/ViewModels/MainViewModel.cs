using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knapsack.Analysis.Experiments;
using Knapsack.Core.Generation;
using Knapsack.Core.Models;
using Knapsack.Core.Solvers;
using Knapsack.DesktopApp.Charts;
using Knapsack.DesktopApp.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Win32;

namespace Knapsack.DesktopApp.ViewModels;

/// <summary>
/// ViewModel principal. Coordena geracao de instancias, execucao dos
/// algoritmos e experimentos, alem de montar as series dos graficos.
/// Nao contem logica de algoritmo: tudo vem de Knapsack.Core/Analysis.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<IKnapsackSolver> _solvers;
    private KnapsackInstance? _currentInstance;
    private IReadOnlyList<ExperimentResult>? _lastExperimentResults;

    public MainViewModel()
    {
        _solvers = new IKnapsackSolver[]
        {
            new ExactExponentialSolver(),
            new DynamicProgrammingSolver(),
            new GreedyHeuristicSolver()
        };
    }

    public Array CapacityModes { get; } = Enum.GetValues(typeof(CapacityMode));

    // --- Parametros de geracao da instancia ---
    [ObservableProperty] private int itemCount = 15;
    [ObservableProperty] private int minWeight = 1;
    [ObservableProperty] private int maxWeight = 20;
    [ObservableProperty] private int minUtility = 1;
    [ObservableProperty] private int maxUtility = 30;
    [ObservableProperty] private CapacityMode selectedCapacityMode = CapacityMode.Medium;
    [ObservableProperty] private bool useSeed;
    [ObservableProperty] private int seedValue = 42;

    // --- Instancia atual ---
    [ObservableProperty] private ObservableCollection<KnapsackItem> instanceItems = new();
    [ObservableProperty] private string instanceSummary = "Nenhuma instancia carregada.";

    // --- Comparacao (instancia atual) ---
    [ObservableProperty] private ObservableCollection<AlgorithmRowViewModel> algorithmRows = new();
    [ObservableProperty] private ISeries[] utilitySeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] utilityXAxes = Array.Empty<Axis>();
    [ObservableProperty] private ISeries[] timeSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] timeXAxes = Array.Empty<Axis>();

    // --- Experimentos / escalabilidade ---
    [ObservableProperty] private int expStartN = 5;
    [ObservableProperty] private int expEndN = 22;
    [ObservableProperty] private int expStep = 1;
    [ObservableProperty] private int expRepetitions = 5;
    [ObservableProperty] private CapacityMode expCapacityMode = CapacityMode.Medium;
    [ObservableProperty] private ISeries[] timeVsNSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] timeVsNXAxes = Array.Empty<Axis>();
    [ObservableProperty] private ISeries[] errorVsNSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] errorVsNXAxes = Array.Empty<Axis>();

    // --- Testes ---
    [ObservableProperty] private string testSummary = "Os testes ainda nao foram executados.";
    [ObservableProperty] private string testOutput = string.Empty;

    /// <summary>Texto explicativo exibido na aba de testes.</summary>
    public string TestExplanation =>
        "Os testes automatizados ficam no projeto Knapsack.Core.Tests e usam o framework xUnit. " +
        "Ao clicar em \"Rodar testes\", a aplicacao executa o comando 'dotnet test' nesse projeto " +
        "(compilando-o se necessario) e mostra a saida completa abaixo.\n\n" +
        "O que e verificado:\n" +
        "  - Caso classico com otimo conhecido: capacidade 50 e itens (10,60),(20,100),(30,120); o otimo deve ser 220.\n" +
        "  - Equivalencia Exato x Programacao Dinamica: em instancias pequenas aleatorias, a utilidade otima dos dois deve ser identica.\n" +
        "  - Garantias da heuristica gulosa: sempre respeita a capacidade, nunca supera o otimo e nao lanca excecoes para entradas validas.\n" +
        "  - Validacoes dos modelos: peso > 0, utilidade >= 0, capacidade > 0, lista de itens nao vazia e Ids sem duplicidade.\n\n" +
        "Ao final, o resumo indica se todos passaram (codigo de saida 0) ou se houve falhas.";

    // --- Estado geral ---
    [ObservableProperty] private string statusMessage = "Pronto.";
    [ObservableProperty] private bool isBusy;

    public bool NotBusy => !IsBusy;
    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(NotBusy));

    [RelayCommand]
    private void GenerateInstance()
    {
        try
        {
            var generator = UseSeed
                ? new KnapsackInstanceGenerator(SeedValue)
                : new KnapsackInstanceGenerator();

            var instance = generator.GenerateRandomInstance(
                ItemCount, MinWeight, MaxWeight, MinUtility, MaxUtility, SelectedCapacityMode);

            SetInstance(instance);
            StatusMessage = $"Instancia gerada: {instance.ItemCount} itens, capacidade {instance.Capacity}.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Erro ao gerar instancia: " + ex.Message;
        }
    }

    [RelayCommand]
    private void LoadInstance()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Instancia JSON (*.json)|*.json|Todos os arquivos (*.*)|*.*",
            Title = "Carregar instancia"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var instance = InstanceIO.Load(dialog.FileName);
            SetInstance(instance);
            StatusMessage = $"Instancia carregada de {dialog.FileName}.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Erro ao carregar: " + ex.Message;
        }
    }

    [RelayCommand]
    private void SaveInstance()
    {
        if (_currentInstance is null)
        {
            StatusMessage = "Nenhuma instancia para salvar.";
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Instancia JSON (*.json)|*.json",
            Title = "Salvar instancia",
            FileName = _currentInstance.Name + ".json"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            InstanceIO.Save(_currentInstance, dialog.FileName);
            StatusMessage = $"Instancia salva em {dialog.FileName}.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Erro ao salvar: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task RunAllAsync()
    {
        if (_currentInstance is null)
        {
            StatusMessage = "Gere ou carregue uma instancia primeiro.";
            return;
        }

        var instance = _currentInstance;
        var solvers = _solvers;

        IsBusy = true;
        StatusMessage = "Executando algoritmos...";
        try
        {
            var solutions = await Task.Run(() =>
            {
                var list = new List<KnapsackSolution>();
                foreach (var solver in solvers)
                {
                    try { list.Add(solver.Solve(instance)); }
                    catch (InvalidOperationException) { /* nao aplicavel a esta instancia */ }
                }
                return list;
            });

            int? optimal = solutions
                .Where(s => s.IsOptimal)
                .Select(s => (int?)s.TotalUtility)
                .FirstOrDefault();

            AlgorithmRows = new ObservableCollection<AlgorithmRowViewModel>(
                solutions.Select(s => new AlgorithmRowViewModel(s, optimal)));

            var (uSeries, uAxes) = ChartFactory.Columns(
                solutions.Select(s => (s.AlgorithmName, (double)s.TotalUtility)).ToList());
            UtilitySeries = uSeries;
            UtilityXAxes = uAxes;

            var (tSeries, tAxes) = ChartFactory.Columns(
                solutions.Select(s => (s.AlgorithmName, s.ElapsedMilliseconds)).ToList());
            TimeSeries = tSeries;
            TimeXAxes = tAxes;

            StatusMessage = optimal is null
                ? $"{solutions.Count} algoritmo(s) executado(s). (Otimo nao calculado.)"
                : $"{solutions.Count} algoritmo(s) executado(s). Otimo = {optimal}.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RunExperimentsAsync()
    {
        if (ExpStartN < 1 || ExpEndN < ExpStartN || ExpStep < 1)
        {
            StatusMessage = "Parametros de varredura invalidos (verifique n inicial/final/passo).";
            return;
        }

        var nValues = new List<int>();
        for (int n = ExpStartN; n <= ExpEndN; n += ExpStep)
            nValues.Add(n);

        int repetitions = ExpRepetitions < 1 ? 1 : ExpRepetitions;
        var mode = ExpCapacityMode;
        var solvers = _solvers;

        IsBusy = true;
        StatusMessage = $"Rodando experimentos ({nValues.Count} tamanhos x {repetitions})...";
        try
        {
            var result = await Task.Run(() => RunScalability(nValues, repetitions, mode, solvers));

            _lastExperimentResults = result.AllResults;

            var (timeSeries, timeAxes) = ChartFactory.Lines(nValues, result.TimeSeriesData, "n (itens)");
            TimeVsNSeries = timeSeries;
            TimeVsNXAxes = timeAxes;

            var (errorSeries, errorAxes) = ChartFactory.Lines(nValues, result.ErrorSeriesData, "n (itens)");
            ErrorVsNSeries = errorSeries;
            ErrorVsNXAxes = errorAxes;

            StatusMessage = $"Experimentos concluidos: {result.AllResults.Count} execucoes. Use 'Exportar CSV' para salvar.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ExportCsv()
    {
        if (_lastExperimentResults is null || _lastExperimentResults.Count == 0)
        {
            StatusMessage = "Rode os experimentos antes de exportar.";
            return;
        }

        try
        {
            var directory = ResolveResultsDirectory();
            var path = Path.Combine(directory, $"resultados-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
            CsvResultExporter.Export(_lastExperimentResults, path);
            StatusMessage = $"CSV exportado: {path}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Erro ao exportar: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task RunTestsAsync()
    {
        IsBusy = true;
        TestOutput = string.Empty;
        TestSummary = "Executando 'dotnet test'... isso pode levar alguns segundos.";
        StatusMessage = "Rodando testes...";
        try
        {
            var result = await TestRunnerService.RunAsync();
            TestOutput = result.Output;
            TestSummary = result.Success
                ? "Todos os testes passaram."
                : "Ha testes falhando - veja os detalhes abaixo.";
            StatusMessage = result.Success ? "Testes concluidos com sucesso." : "Testes concluidos com falhas.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetInstance(KnapsackInstance instance)
    {
        _currentInstance = instance;
        InstanceItems = new ObservableCollection<KnapsackItem>(instance.Items);
        InstanceSummary =
            $"{instance.Name}  |  Itens: {instance.ItemCount}  |  Capacidade: {instance.Capacity}  |  Soma dos pesos: {instance.TotalWeight}";

        // Limpa resultados antigos para evitar confusao com a nova instancia.
        AlgorithmRows = new ObservableCollection<AlgorithmRowViewModel>();
        UtilitySeries = Array.Empty<ISeries>();
        TimeSeries = Array.Empty<ISeries>();
    }

    private static ScalabilityResult RunScalability(
        List<int> nValues,
        int repetitions,
        CapacityMode mode,
        IReadOnlyList<IKnapsackSolver> solvers)
    {
        var runner = new ExperimentRunner(solvers);
        var generator = new KnapsackInstanceGenerator();
        var allResults = new List<ExperimentResult>();

        var algorithmNames = solvers.Select(s => s.Name).ToList();
        var timeByAlgorithm = algorithmNames.ToDictionary(a => a, _ => new double?[nValues.Count]);
        var errorByAlgorithm = algorithmNames.ToDictionary(a => a, _ => new double?[nValues.Count]);

        for (int idx = 0; idx < nValues.Count; idx++)
        {
            int n = nValues[idx];
            var instances = new List<KnapsackInstance>(repetitions);
            for (int r = 1; r <= repetitions; r++)
                instances.Add(generator.GenerateRandomInstance(n, 1, 20, 1, 30, mode, $"n{n}-r{r}"));

            var results = runner.Run(instances);
            allResults.AddRange(results);

            foreach (var aggregate in ExperimentRunner.Aggregate(results))
            {
                if (timeByAlgorithm.TryGetValue(aggregate.Algorithm, out var times))
                    times[idx] = aggregate.AverageElapsedMs;

                if (aggregate.AverageRelativeError.HasValue &&
                    errorByAlgorithm.TryGetValue(aggregate.Algorithm, out var errors))
                    errors[idx] = aggregate.AverageRelativeError.Value * 100.0;
            }
        }

        var timeSeriesData = algorithmNames.Select(a => (a, timeByAlgorithm[a])).ToList();
        var errorSeriesData = algorithmNames.Select(a => (a, errorByAlgorithm[a])).ToList();
        return new ScalabilityResult(allResults, timeSeriesData, errorSeriesData);
    }

    private static string ResolveResultsDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "KnapsackSolution.sln")))
                return Path.Combine(dir.FullName, "data", "results");
            dir = dir.Parent;
        }
        return Path.Combine(Directory.GetCurrentDirectory(), "data", "results");
    }

    private sealed record ScalabilityResult(
        List<ExperimentResult> AllResults,
        List<(string Algorithm, double?[] Values)> TimeSeriesData,
        List<(string Algorithm, double?[] Values)> ErrorSeriesData);
}
