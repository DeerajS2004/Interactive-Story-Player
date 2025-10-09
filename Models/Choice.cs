using System.Text.Json.Serialization;

namespace InteractiveStoryPlayer.Models;

public class Choice
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("next")]
    public string Next { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }
}