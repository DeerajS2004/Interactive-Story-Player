using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InteractiveStoryPlayer.Models;

public class StoryRoot
{
    [JsonPropertyName("meta")]
    public StoryMeta? Meta { get; set; }

    [JsonPropertyName("start")]
    public string Start { get; set; } = string.Empty;

    [JsonPropertyName("scenes")]
    public Dictionary<string, Scene> Scenes { get; set; } = new();
}

public class StoryMeta
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;
}