using System.Text.Json;
using TheFipster.ActivityAggregator.App.Models;

namespace TheFipster.ActivityAggregator.App.Services;

public class StorageService
{
    private readonly string _filePath;

    public StorageService()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "weights.json");
    }

    public async Task<List<WeightEntry>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<WeightEntry>();

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<WeightEntry>>(json) ?? new List<WeightEntry>();
    }

    public async Task SaveAsync(List<WeightEntry> entries)
    {
        var json = JsonSerializer.Serialize(
            entries,
            new JsonSerializerOptions { WriteIndented = true }
        );
        await File.WriteAllTextAsync(_filePath, json);
    }

    public string GetFilePath() => _filePath;
}
