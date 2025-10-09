using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using InteractiveStoryPlayer.Models;

namespace InteractiveStoryPlayer.Services;

public class StoryLoader
{
    private FileSystemWatcher? _fileWatcher;
    private string? _currentFilePath;

    public event EventHandler? StoryFileChanged;

    public async Task<StoryRoot?> LoadStoryAsync(string filePath)
    {
        try
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var story = JsonSerializer.Deserialize<StoryRoot>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (story != null)
            {
                SetupFileWatcher(filePath);
                return story;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading story: {ex.Message}");
        }
        return null;
    }

    private void SetupFileWatcher(string filePath)
    {
        _fileWatcher?.Dispose();
        _currentFilePath = filePath;

        var directory = Path.GetDirectoryName(filePath);
        var fileName = Path.GetFileName(filePath);

        if (directory != null && fileName != null)
        {
            _fileWatcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += (sender, e) =>
            {
                Task.Delay(500).ContinueWith(_ =>
                {
                    StoryFileChanged?.Invoke(this, EventArgs.Empty);
                });
            };
        }
    }

    public string? GetCurrentFilePath() => _currentFilePath;

    public string? GetStoryDirectory()
    {
        return _currentFilePath != null ? Path.GetDirectoryName(_currentFilePath) : null;
    }

    public void Dispose()
    {
        _fileWatcher?.Dispose();
    }
}