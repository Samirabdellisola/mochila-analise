using System.Diagnostics;
using System.IO;
using System.Text;

namespace Knapsack.DesktopApp.Services;

public sealed record TestRunResult(bool Success, string Output);

/// <summary>
/// Executa a suite de testes unitarios (xUnit) chamando o SDK do .NET
/// (`dotnet test`) no projeto Knapsack.Core.Tests e captura a saida.
/// </summary>
public static class TestRunnerService
{
    public static async Task<TestRunResult> RunAsync()
    {
        var root = FindSolutionRoot();
        if (root is null)
            return new TestRunResult(false, "Nao foi possivel localizar a solution (KnapsackSolution.sln).");

        var testProject = Path.Combine(root, "tests", "Knapsack.Core.Tests", "Knapsack.Core.Tests.csproj");
        if (!File.Exists(testProject))
            return new TestRunResult(false, $"Projeto de testes nao encontrado em:\n{testProject}");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"test \"{testProject}\" --nologo",
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        var output = new StringBuilder();

        try
        {
            using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            process.OutputDataReceived += (_, e) => { if (e.Data is not null) output.AppendLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data is not null) output.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            return new TestRunResult(process.ExitCode == 0, output.ToString());
        }
        catch (Exception ex)
        {
            return new TestRunResult(false, $"Falha ao iniciar 'dotnet test': {ex.Message}\n\n{output}");
        }
    }

    private static string? FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "KnapsackSolution.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}
