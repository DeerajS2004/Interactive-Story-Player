using System.Collections.Generic;

namespace InteractiveStoryPlayer.Models;

public class Scene
{
    public string Text { get; set; } = string.Empty;
    public string? Image { get; set; } // Keep for backward compatibility
    public List<string>? Images { get; set; } // Add this new property for multiple images
    public List<Choice>? Choices { get; set; }
}
