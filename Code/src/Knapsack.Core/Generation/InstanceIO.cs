using System.Text.Json;
using System.Text.Json.Serialization;
using Knapsack.Core.Models;

namespace Knapsack.Core.Generation;

/// <summary>
/// Leitura e escrita de instâncias em JSON. Usa DTOs internos porque os
/// modelos de domínio são imutáveis e validados no construtor.
/// </summary>
public static class InstanceIO
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public static void Save(KnapsackInstance instance, string path)
    {
        var dto = new InstanceDto
        {
            Name = instance.Name,
            Capacity = instance.Capacity,
            Items = instance.Items
                .Select(i => new ItemDto { Id = i.Id, Weight = i.Weight, Utility = i.Utility })
                .ToList()
        };

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, JsonSerializer.Serialize(dto, Options));
    }

    public static KnapsackInstance Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Arquivo de instância não encontrado.", path);

        var dto = JsonSerializer.Deserialize<InstanceDto>(File.ReadAllText(path), Options)
            ?? throw new InvalidOperationException("Não foi possível desserializar a instância.");

        var items = dto.Items.Select(i => new KnapsackItem(i.Id, i.Weight, i.Utility));
        var instance = new KnapsackInstance(dto.Capacity, items, dto.Name);
        instance.Validate();
        return instance;
    }

    private sealed class InstanceDto
    {
        public string? Name { get; set; }
        public int Capacity { get; set; }
        public List<ItemDto> Items { get; set; } = new();
    }

    private sealed class ItemDto
    {
        public int Id { get; set; }
        public int Weight { get; set; }
        public int Utility { get; set; }
    }
}
