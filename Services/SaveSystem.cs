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

    var json = JsonSerializer.Serialize(saveData);
    var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "InteractiveStoryPlayer", "save.json");
    
    Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
    await File.WriteAllTextAsync(savePath, json);
    
    Console.WriteLine($"Saved scene: {currentSceneId}"); // Debug output
}

public async Task<SaveData?> LoadAsync()
{
    var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "InteractiveStoryPlayer", "save.json");
    
    if (!File.Exists(savePath)) return null;
    
    var json = await File.ReadAllTextAsync(savePath);
    var saveData = JsonSerializer.Deserialize<SaveData>(json);
    
    Console.WriteLine($"Loaded scene: {saveData?.CurrentSceneId}"); // Debug output
    return saveData;
}


    public void ClearSave()
    {
        try
        {
            if (File.Exists(_saveFilePath))
            {
                File.Delete(_saveFilePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing save: {ex.Message}");
        }
    }
}