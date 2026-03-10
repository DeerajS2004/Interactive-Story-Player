using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace InteractiveStoryPlayer.Services;

public class SaveData
{
    public string? StoryFilePath { get; set; }
    public string? CurrentSceneId { get; set; }
    public DateTime SavedDate { get; set; } = DateTime.Now;
}

public class SaveSystem
{
    private readonly string _saveFilePath;

    public SaveSystem()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "InteractiveStoryPlayer");
        Directory.CreateDirectory(appFolder);
        _saveFilePath = Path.Combine(appFolder, "save.json");
    }

    public async Task SaveAsync(string storyFilePath, string currentSceneId)
    {
        var saveData = new SaveData
        {
            StoryFilePath = storyFilePath,
            CurrentSceneId = currentSceneId,
            SavedDate = DateTime.Now
        };

        var json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_saveFilePath, json);
    }

    public async Task<SaveData?> LoadAsync()
    {
        if (!File.Exists(_saveFilePath)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(_saveFilePath);
            return JsonSerializer.Deserialize<SaveData>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading save file: {ex.Message}");
            return null;
        }
    }

    public void ClearSave()
    {
        try
        {
            if (File.Exists(_saveFilePath))
                File.Delete(_saveFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing save: {ex.Message}");
        }
    }
}
