using System.Globalization;
using System.Text;

namespace Knapsack.Analysis.Experiments;

/// <summary>
/// Exporta resultados de experimentos para CSV. Usa cultura invariante para
/// garantir ponto como separador decimal, independente do sistema.
/// </summary>
public static class CsvResultExporter
{
    private const string Header =
        "InstanceName,ItemCount,Capacity,Algorithm,TotalUtility,TotalWeight,ElapsedMs,IsOptimal,RelativeError";

    public static void Export(IEnumerable<ExperimentResult> results, string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Header);

        foreach (var result in results)
        {
            foreach (var run in result.Runs)
            {
                string relativeError = run.RelativeError.HasValue
                    ? run.RelativeError.Value.ToString("F6", CultureInfo.InvariantCulture)
                    : string.Empty;

                sb.Append(Escape(run.InstanceName)).Append(',')
                  .Append(run.ItemCount).Append(',')
                  .Append(run.Capacity).Append(',')
                  .Append(Escape(run.Algorithm)).Append(',')
                  .Append(run.TotalUtility).Append(',')
                  .Append(run.TotalWeight).Append(',')
                  .Append(run.ElapsedMs.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
                  .Append(run.IsOptimal ? "true" : "false").Append(',')
                  .Append(relativeError)
                  .AppendLine();
            }
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, sb.ToString());
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
