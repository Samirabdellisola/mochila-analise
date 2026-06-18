using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Knapsack.DesktopApp.Charts;

/// <summary>
/// Constroi as series e eixos do LiveCharts2 a partir dos resultados,
/// isolando os detalhes de visualizacao das ViewModels.
/// </summary>
public static class ChartFactory
{
    /// <summary>
    /// Grafico de colunas comparando um valor por algoritmo (ex.: utilidade ou tempo).
    /// Cada algoritmo vira uma serie propria para ganhar cor e legenda.
    /// </summary>
    public static (ISeries[] Series, Axis[] XAxes) Columns(IReadOnlyList<(string Algorithm, double Value)> data)
    {
        var labels = data.Select(d => AlgorithmPalette.ShortName(d.Algorithm)).ToArray();
        var series = new List<ISeries>(data.Count);

        for (int i = 0; i < data.Count; i++)
        {
            var values = new double?[data.Count];
            values[i] = data[i].Value;

            var color = AlgorithmPalette.ColorFor(data[i].Algorithm);
            series.Add(new ColumnSeries<double?>
            {
                Name = labels[i],
                Values = values,
                IgnoresBarPosition = true,
                Fill = new SolidColorPaint(color),
                DataLabelsPaint = new SolidColorPaint(new SKColor(0x33, 0x33, 0x33)),
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("0.###")
            });
        }

        var xAxis = new Axis { Labels = labels };
        return (series.ToArray(), new[] { xAxis });
    }

    /// <summary>
    /// Grafico de linhas (escalabilidade): uma serie por algoritmo ao longo
    /// dos valores de n. Valores nulos viram lacunas (ex.: exato em n grande).
    /// </summary>
    public static (ISeries[] Series, Axis[] XAxes) Lines(
        IReadOnlyList<int> nValues,
        IReadOnlyList<(string Algorithm, double?[] Values)> seriesData,
        string xAxisName)
    {
        var labels = nValues.Select(n => n.ToString()).ToArray();

        var series = seriesData.Select(s =>
        {
            var color = AlgorithmPalette.ColorFor(s.Algorithm);
            return (ISeries)new LineSeries<double?>
            {
                Name = AlgorithmPalette.ShortName(s.Algorithm),
                Values = s.Values,
                Stroke = new SolidColorPaint(color) { StrokeThickness = 2.5f },
                Fill = null,
                GeometrySize = 8,
                GeometryFill = new SolidColorPaint(color),
                GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 1.5f }
            };
        }).ToArray();

        var xAxis = new Axis { Labels = labels, Name = xAxisName };
        return (series, new[] { xAxis });
    }
}
