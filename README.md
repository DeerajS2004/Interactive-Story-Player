# Interactive Story Player

A cross-platform desktop application for playing branching narrative stories, built with C# and Avalonia UI. Stories are defined entirely in JSON — no recompilation needed to add new stories or change existing ones.

![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Avalonia](https://img.shields.io/badge/Avalonia-11.3.4-teal)
![License](https://img.shields.io/badge/license-MIT-green)

---

## Features

- **JSON-driven stories** — write or swap stories without touching code
- **Branching choices** — multiple choices per scene, each leading to a different path
- **Multiple endings** — scenes with no choices display an ending screen with a replay option
- **Save & load** — progress is saved to `AppData` and can be resumed at any time
- **Multiple images per scene** — each scene supports an `images` array or a single `image` field
- **Hot reload** — editing the story JSON while the app is open automatically reloads it
- **Dark theme** — fully styled dark UI with hover and press effects on all buttons

---

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run

```bash
git clone https://github.com/DeerajS2004/Interactive-Story-Player.git
cd Interactive-Story-Player
dotnet run
```

### Build

```bash
dotnet build -c Release
```

---

## Project Structure

```
InteractiveStoryPlayer/
├── Assets/
│   ├── story.json          # Default story file
│   └── images/             # Scene images (referenced in story.json)
├── Engine/
│   └── StoryEngine.cs      # Core engine: scene state, navigation, events
├── Models/
│   ├── Scene.cs            # Scene data model
│   ├── Choice.cs           # Choice data model
│   └── StoryRoot.cs        # Story root + metadata model
├── Services/
│   ├── StoryLoader.cs      # JSON loader with file watcher (hot reload)
│   └── SaveSystem.cs       # Save/load progress to AppData
└── UI/
    ├── MainWindow.axaml     # UI layout and styles
    └── MainWindow.axaml.cs  # UI logic and event handling
```

---

## Story JSON Format

Stories are plain JSON files. Place them anywhere and load via the app, or replace `Assets/story.json` as the default.

```json
{
  "meta": {
    "title": "My Story",
    "version": "1.0",
    "author": "Your Name"
  },
  "start": "scene_id",
  "scenes": {
    "scene_id": {
      "text": "The scene description shown to the player.",
      "images": ["images/scene.jpg"],
      "choices": [
        { "text": "Choice label", "next": "another_scene_id" },
        { "text": "Another choice", "next": "ending_scene_id" }
      ]
    },
    "ending_scene_id": {
      "text": "The story ends here.",
      "images": ["images/ending.jpg"],
      "choices": []
    }
  }
}
```

**Notes:**
- `start` — the ID of the first scene to display
- `images` — array of paths relative to the story JSON file's directory; falls back to a single `image` field for compatibility
- `choices: []` — an empty choices array marks a scene as an ending; the app shows a "Play Again" button automatically

---

## Save File Location

| OS | Path |
|----|------|
| Windows | `%APPDATA%\InteractiveStoryPlayer\save.json` |
| Linux | `~/.config/InteractiveStoryPlayer/save.json` |
| macOS | `~/Library/Application Support/InteractiveStoryPlayer/save.json` |

---

## Built With

- [C#](https://learn.microsoft.com/en-us/dotnet/csharp/) — .NET 8.0
- [Avalonia UI](https://avaloniaui.net/) — 11.3.4, cross-platform UI framework

---

## License

MIT — see [LICENSE](LICENSE) for details.
